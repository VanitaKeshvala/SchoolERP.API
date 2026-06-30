// wwwroot/js/sidebarActiveMenu.js
//
// Highlights the sidebar menu link that matches the current page URL, and
// expands any parent submenu(s) it lives under, so the active item is
// visible without the user needing to click the parent first.
//
// This is required because the Menu ViewComponent's RenderMenus(parentId,
// depth) method (Views/Shared/Components/Menu/Default.cshtml) renders pure
// navigation markup only — it never adds an "active" class to anything.
// This script is the only thing that adds active/expanded state, and it
// adds 100% of it; it does not assume any class already exists.
//
// Matches the exact markup RenderMenus produces:
//
//   Leaf (no children):
//     <li><a href="@menu.MenuURL">[icon if depth==0]<span>Name</span></a></li>
//
//   Parent with children (depth > 0):
//     <li class="submenu">
//       <a href="javascript:void(0);"><span>Name</span><span class="menu-arrow ..."></span></a>
//       <ul> ...children via RenderMenus(menu.MenuID, depth+1)... </ul>
//     </li>
//
//   Parent with children (depth == 0) — note the extra <li><ul> wrapper:
//     <li><ul><li class="submenu">
//       <a href="javascript:void(0);">[icon]<span>Name</span><span class="menu-arrow ..."></span></a>
//       <ul> ...children... </ul>
//     </li></ul></li>
//
// Include this file in _Layout.cshtml AFTER the sidebar menu markup exists
// in the DOM, e.g. right before </body>:
//
//   <script src="~/js/sidebarActiveMenu.js"></script>

(function () {
    function highlightActiveMenu() {
        const sidebarMenu = document.getElementById('sidebar-menu');
        if (!sidebarMenu) return;

        // Safety-net CSS: guarantees an expanded submenu is actually visible
        // even if style.css doesn't already define how "open" submenus look.
        if (!document.getElementById('sidebarActiveMenuStyle')) {
            const style = document.createElement('style');
            style.id = 'sidebarActiveMenuStyle';
            style.textContent = `
                #sidebar-menu li.submenu.menu-open > ul { display: block !important; }
            `;
            document.head.appendChild(style);
        }

        function normalizePath(path) {
            if (!path) return '/';
            path = path.split('?')[0].split('#')[0];
            if (path.length > 1 && path.endsWith('/')) {
                path = path.slice(0, -1);
            }
            return path.toLowerCase() || '/';
        }

        const currentPath = normalizePath(window.location.pathname);

        // First path segment = the MVC controller, e.g. "/MenuMaster/AddMenu"
        // -> "menumaster". Used as a fallback below for pages (like an
        // Add/Edit form) that aren't themselves in the sidebar, but share a
        // controller with a sidebar link that IS (e.g. a "Menu List" link
        // pointing at /MenuMaster/Index).
        function firstSegment(path) {
            const parts = path.split('/').filter(Boolean);
            return parts.length ? parts[0] : '';
        }
        const currentController = firstSegment(currentPath);

        // Only real navigable links — RenderMenus uses
        // href="javascript:void(0);" for every parent/submenu toggle, so
        // those are excluded from ever being treated as the active page.
        const links = Array.from(sidebarMenu.querySelectorAll('a[href]')).filter(a => {
            const href = a.getAttribute('href') || '';
            return href && !href.toLowerCase().startsWith('javascript:') && href !== '#';
        });

        // Three-tier match, each tier only used if a better one wasn't found:
        //   1) Exact path match.
        //   2) Longest href the current path is "under" (e.g. /Students/Details/5
        //      under /Students/Index).
        //   3) Same controller (first path segment) — handles pages like
        //      /MenuMaster/AddMenu that aren't in the sidebar themselves but
        //      belong to the same controller as a sidebar link such as
        //      /MenuMaster/Index ("Menu List").
        let bestLink = null;
        let bestLen = -1;
        let controllerFallbackLink = null;

        links.forEach(a => {
            let href;
            try {
                href = normalizePath(new URL(a.getAttribute('href'), window.location.origin).pathname);
            } catch {
                return;
            }
            if (href === '/' && currentPath !== '/') return; // don't let "/" swallow every page

            if (href === currentPath) {
                bestLink = a;
                bestLen = href.length + 1; // exact match always wins
            } else if (currentPath.startsWith(href + '/') && href.length > bestLen) {
                bestLink = a;
                bestLen = href.length;
            } else if (!controllerFallbackLink && currentController && firstSegment(href) === currentController) {
                controllerFallbackLink = a;
            }
        });

        if (!bestLink) bestLink = controllerFallbackLink;
        if (!bestLink) return;

        // Mark the matched leaf link + its own <li> active.
        bestLink.classList.add('active');
        const leafLi = bestLink.closest('li');
        if (leafLi) leafLi.classList.add('active');

        // Walk every ancestor <li> (any depth — RenderMenus recursion is
        // unbounded) and, for each submenu parent, mark it active and
        // expand its <ul> so the active leaf is visible on page load.
        let node = bestLink.parentElement;
        while (node && node !== sidebarMenu) {
            if (node.tagName === 'LI') {
                node.classList.add('active');

                const parentLink = node.querySelector(':scope > a');
                const submenu = node.querySelector(':scope > ul');
                if (parentLink) {
                    parentLink.classList.add('active', 'subdrop');
                }
                if (submenu) {
                    node.classList.add('menu-open', 'submenu-open');
                    submenu.style.display = 'block';
                }
            }
            node = node.parentElement;
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', highlightActiveMenu);
    } else {
        highlightActiveMenu();
    }
})();