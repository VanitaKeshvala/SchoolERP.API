using Microsoft.Extensions.Configuration;
using SchoolERP.Net.Models;
using SchoolERP.Net.Models.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchoolERP.Net.Services.Clients
{
    public class StudentCertificateClientService : BaseApiClient, IStudentCertificateClientService
    {
        public StudentCertificateClientService(HttpClient httpClient, IConfiguration configuration) : base(httpClient)
        {
        }

        public async Task<ApiResponse<List<StudentCertificateViewModel>>> GetAll()
        {
            return await GetAsync<List<StudentCertificateViewModel>>("api/StudentCertificateApi/GetAll");
        }

        public async Task<ApiResponse<StudentCertificateViewModel>> GetByID(int id)
        {
            return await GetAsync<StudentCertificateViewModel>($"api/StudentCertificateApi/GetByID/{id}");
        }

        public async Task<ApiResponse<int>> Upsert(StudentCertificateUpsertRequest request)
        {
            return await PostAsync<int>("api/StudentCertificateApi/Upsert", request);
        }

        public async Task<ApiResponse<int>> Delete(List<int> id)
        {
            return await PostAsync<int>($"api/StudentCertificateApi/Delete", id);
        }

        public async Task<ApiResponse<int>> ToggleStatus(int id, bool isActive)
        {
            return await PostAsync<int>($"api/StudentCertificateApi/ToggleStatus?id={id}&isActive={isActive}", null);
        }

        public async Task<ApiResponse<string>> Generate(int studentId, int certificateId)
        {
            return await GetAsync<string>($"api/StudentCertificateApi/Generate?studentId={studentId}&certificateId={certificateId}");
        }
    }
}
