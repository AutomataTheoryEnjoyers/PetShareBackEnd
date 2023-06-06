﻿using PetShare.Models.Reports;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Reports;

public interface IReportQuery
{
    Task<Result<ReportPage>> GetNewReportsPageAsync(int pageNumber, int pageSize, CancellationToken token = default);
}
