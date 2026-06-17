/**
 * Shared inline form validation (Bootstrap is-invalid + invalid-feedback), aligned with User wizard pattern.
 */
(function (w) {
    'use strict';

    function byId(id) {
        return id ? document.getElementById(id) : null;
    }

    w.InlineFormValidation = {
        setFieldError: function (inputId, errorElementId, message) {
            var input = byId(inputId);
            var err = byId(errorElementId);
            if (input) input.classList.add('is-invalid');
            if (err) {
                err.textContent = message || 'Invalid value.';
                err.classList.add('d-block');
            }
        },

        clearFieldError: function (inputId, errorElementId) {
            var input = byId(inputId);
            var err = byId(errorElementId);
            if (input) input.classList.remove('is-invalid');
            if (err) {
                err.textContent = '';
                err.classList.remove('d-block');
            }
        },

        /** fieldMap: { inputId: errorElementId, ... } */
        clearMap: function (fieldMap) {
            var self = this;
            if (!fieldMap) return;
            Object.keys(fieldMap).forEach(function (inputId) {
                self.clearFieldError(inputId, fieldMap[inputId]);
            });
        },

        bindAutoClear: function (fieldMap) {
            var self = this;
            if (!fieldMap) return;
            Object.keys(fieldMap).forEach(function (inputId) {
                var el = byId(inputId);
                if (!el) return;
                var errId = fieldMap[inputId];
                var handler = function () {
                    self.clearFieldError(inputId, errId);
                };
                el.addEventListener('input', handler);
                el.addEventListener('change', handler);
            });
        },

        setNotice: function (elementId, message, type) {
            if (!message) return;
            
            // If type is not provided, try to infer it from elementId or message
            if (!type) {
                var msg = message.toLowerCase();
                var eid = (elementId || '').toLowerCase();
                if (msg.includes('success') || msg.includes('saved') || msg.includes('updated')) type = 'success';
                else if (msg.includes('error') || msg.includes('failed') || msg.includes('invalid') || eid.includes('err')) type = 'error';
                else type = 'info';
            }

            if (typeof window.showToast === 'function') {
                window.showToast(message, type);
            } else {
                // Fallback to legacy behavior if global toast not ready
                var el = byId(elementId);
                if (el) {
                    el.textContent = message;
                    el.classList.remove('d-none');
                }
            }
        },

        clearNotice: function (elementId) {
            var el = byId(elementId);
            if (el) {
                el.textContent = '';
                el.classList.add('d-none');
            }
        },

        isNonEmpty: function (v) {
            return v != null && String(v).trim().length > 0;
        },

        isValidEmail: function (email) {
            if (!email || !String(email).trim()) return true;
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email).trim());
        },

        isValidMobile: function (mobile) {
            if (!mobile || !String(mobile).trim()) return true;
            return /^\d{10}$/.test(String(mobile).trim());
        },

        validateMobile: function (inputId, errorElementId, isRequired) {
            var val = byId(inputId).value.trim();
            if (isRequired && !this.isNonEmpty(val)) {
                this.setFieldError(inputId, errorElementId, 'Mobile number is required.');
                return false;
            }
            if (val && !this.isValidMobile(val)) {
                this.setFieldError(inputId, errorElementId, 'Mobile number must be 10 digits.');
                return false;
            }
            this.clearFieldError(inputId, errorElementId);
            return true;
        },

        validateEmail: function (inputId, errorElementId, isRequired) {
            var val = byId(inputId).value.trim();
            if (isRequired && !this.isNonEmpty(val)) {
                this.setFieldError(inputId, errorElementId, 'Email is required.');
                return false;
            }
            if (val && !this.isValidEmail(val)) {
                this.setFieldError(inputId, errorElementId, 'Invalid email format.');
                return false;
            }
            this.clearFieldError(inputId, errorElementId);
            return true;
        }
    };
})(window);
