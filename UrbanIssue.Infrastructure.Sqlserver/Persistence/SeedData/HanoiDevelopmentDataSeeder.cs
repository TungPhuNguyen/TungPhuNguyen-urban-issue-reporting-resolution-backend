using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Domain.Constants;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Seed;

public sealed class HanoiDevelopmentDataSeeder
{
    private const string DemoPassword =
        "Password@123";

    private const string HanoiRootName =
        "Thành phố Hà Nội";

    private const string HanoiRootCode =
        "HN-CITY";

    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly DefaultSlaSettings _defaultSlaSettings;
    private readonly ILogger<HanoiDevelopmentDataSeeder> _logger;

    public HanoiDevelopmentDataSeeder(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<DefaultSlaSettings> defaultSlaOptions,
        ILogger<HanoiDevelopmentDataSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _defaultSlaSettings = defaultSlaOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        /*
         * Chỉ nên gọi seeder này trong Development.
         * Migration được áp dụng trước khi thêm dữ liệu.
         */
        await _dbContext.Database.MigrateAsync(
            cancellationToken);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var currentTime = DateTime.UtcNow;

            var categories =
                await SeedCategoriesAsync(
                    currentTime,
                    cancellationToken);

            await SeedSlaConfigsAsync(
                categories,
                currentTime,
                cancellationToken);

            var areas =
                await SeedAreasAsync(
                    currentTime,
                    cancellationToken);

            var departments =
                await SeedDepartmentsAsync(
                    currentTime,
                    cancellationToken);

            await SeedRoutingRulesAsync(
                categories,
                areas,
                departments,
                currentTime,
                cancellationToken);

            await SeedUsersAsync(
                departments,
                currentTime,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            _logger.LogInformation(
                "Đã seed dữ liệu Development cho Hà Nội.");
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }

    private async Task<
        Dictionary<string, Category>>
        SeedCategoriesAsync(
            DateTime currentTime,
            CancellationToken cancellationToken)
    {
        var existingCategories =
            await _dbContext.Categories
                .ToListAsync(cancellationToken);

        var categoriesByName =
            existingCategories.ToDictionary(
                category => category.Name,
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in CategorySeeds)
        {
            if (categoriesByName.ContainsKey(
                    seed.Name))
            {
                continue;
            }

            var category = new Category
            {
                Name = seed.Name,
                Description = seed.Description,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

            _dbContext.Categories.Add(category);

            categoriesByName.Add(
                category.Name,
                category);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return categoriesByName;
    }

    private async Task SeedSlaConfigsAsync(
        IReadOnlyDictionary<string, Category> categories,
        DateTime currentTime,
        CancellationToken cancellationToken)
    {
        var existingConfigs =
            await _dbContext.SLAConfigs
                .AsNoTracking()
                .Select(config => new
                {
                    config.CategoryId,
                    config.Priority
                })
                .ToListAsync(cancellationToken);

        var existingKeys =
            existingConfigs
                .Select(config => (
                    config.CategoryId,
                    config.Priority))
                .ToHashSet();

        foreach (var category in categories.Values)
        {
            AddSlaConfigIfMissing(
                category,
                ReportPriority.Low,
                _defaultSlaSettings.LowHours,
                existingKeys,
                currentTime);

            AddSlaConfigIfMissing(
                category,
                ReportPriority.Medium,
                _defaultSlaSettings.MediumHours,
                existingKeys,
                currentTime);

            AddSlaConfigIfMissing(
                category,
                ReportPriority.High,
                _defaultSlaSettings.HighHours,
                existingKeys,
                currentTime);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private void AddSlaConfigIfMissing(
        Category category,
        ReportPriority priority,
        int durationHours,
        ISet<(int CategoryId, ReportPriority Priority)>
            existingKeys,
        DateTime currentTime)
    {
        var key = (
            CategoryId: category.Id,
            Priority: priority);

        if (existingKeys.Contains(key))
        {
            return;
        }

        _dbContext.SLAConfigs.Add(
            new SLAConfig
            {
                CategoryId = category.Id,
                Priority = priority,
                DurationHours = durationHours,
                CreatedAt = currentTime,
                UpdatedAt = null
            });

        existingKeys.Add(key);
    }

    private async Task<
        Dictionary<string, Area>>
        SeedAreasAsync(
            DateTime currentTime,
            CancellationToken cancellationToken)
    {
        var existingAreas =
            await _dbContext.Areas
                .ToListAsync(cancellationToken);

        var areasByCode =
            existingAreas
                .Where(area =>
                    !string.IsNullOrWhiteSpace(area.Code))
                .GroupBy(
                    area => area.Code!,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

        var areasByName =
            existingAreas
                .GroupBy(
                    area => area.Name,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

        /*
         * Từ 01/07/2025, Hà Nội vận hành mô hình chính quyền
         * địa phương hai cấp. Ứng dụng vẫn cần ParentAreaId,
         * vì vậy dùng "Thành phố Hà Nội" làm node gốc và
         * 126 Phường/Xã chính thức làm node con.
         */
        if (!areasByCode.TryGetValue(
                HanoiRootCode,
                out var hanoi)
            && !areasByName.TryGetValue(
                HanoiRootName,
                out hanoi))
        {
            hanoi = new Area
            {
                Name = HanoiRootName,
                Code = HanoiRootCode,
                ParentAreaId = null,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

            _dbContext.Areas.Add(hanoi);
        }
        else
        {
            hanoi.Name = HanoiRootName;
            hanoi.Code = HanoiRootCode;
            hanoi.ParentAreaId = null;
            hanoi.IsActive = true;
            hanoi.UpdatedAt = currentTime;
        }

        areasByCode[HanoiRootCode] = hanoi;
        areasByName[HanoiRootName] = hanoi;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        foreach (var unitSeed in AdministrativeUnitSeeds)
        {
            var firstSpace =
                unitSeed.Name.IndexOf(' ');

            var legacyName =
                firstSpace >= 0
                    ? unitSeed.Name[(firstSpace + 1)..]
                    : unitSeed.Name;

            if (!areasByCode.TryGetValue(
                    unitSeed.Code,
                    out var unit)
                && !areasByName.TryGetValue(
                    unitSeed.Name,
                    out unit)
                && !areasByName.TryGetValue(
                    legacyName,
                    out unit))
            {
                unit = new Area
                {
                    Name = unitSeed.Name,
                    Code = unitSeed.Code,
                    ParentAreaId = hanoi.Id,
                    IsActive = true,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                };

                _dbContext.Areas.Add(unit);
            }
            else
            {
                unit.Name = unitSeed.Name;
                unit.Code = unitSeed.Code;
                unit.ParentAreaId = hanoi.Id;
                unit.IsActive = true;
                unit.UpdatedAt = currentTime;
            }

            areasByCode[unitSeed.Code] = unit;
            areasByName[unitSeed.Name] = unit;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        /*
         * Vô hiệu hóa dữ liệu seed cũ không còn thuộc danh sách
         * 126 đơn vị hiện hành. Không xóa để giữ khóa ngoại của
         * Report, RoutingRule và dữ liệu demo đã tồn tại.
         */
        var currentAreaIds =
            AdministrativeUnitSeeds
                .Select(seed =>
                    areasByCode[seed.Code].Id)
                .Append(hanoi.Id)
                .ToHashSet();

        foreach (var area in existingAreas)
        {
            var isOldSeedArea =
                area.Code?.StartsWith(
                    "HN-D-",
                    StringComparison.OrdinalIgnoreCase)
                    == true
                || area.Code?.StartsWith(
                    "HN-W-",
                    StringComparison.OrdinalIgnoreCase)
                    == true
                || area.Code?.StartsWith(
                    "HN-C-",
                    StringComparison.OrdinalIgnoreCase)
                    == true;

            if (!isOldSeedArea
                || currentAreaIds.Contains(area.Id))
            {
                continue;
            }

            area.IsActive = false;
            area.UpdatedAt = currentTime;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return areasByCode;
    }

    private async Task<
        Dictionary<string, Department>>
        SeedDepartmentsAsync(
            DateTime currentTime,
            CancellationToken cancellationToken)
    {
        var existingDepartments =
            await _dbContext.Departments
                .ToListAsync(cancellationToken);

        var departmentsByName =
            existingDepartments.ToDictionary(
                department => department.Name,
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in DepartmentSeeds)
        {
            if (departmentsByName.ContainsKey(
                    seed.Name))
            {
                continue;
            }

            var department = new Department
            {
                Name = seed.Name,
                Description = seed.Description,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

            _dbContext.Departments.Add(department);

            departmentsByName.Add(
                department.Name,
                department);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return departmentsByName;
    }

    private async Task SeedRoutingRulesAsync(
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, Area> areas,
        IReadOnlyDictionary<string, Department> departments,
        DateTime currentTime,
        CancellationToken cancellationToken)
    {
        var existingRules =
            await _dbContext.RoutingRules
                .ToListAsync(cancellationToken);

        var currentAreaIds =
            AdministrativeUnitSeeds
                .Select(seed =>
                    areas[seed.Code].Id)
                .ToHashSet();

        /*
         * Rule của khu vực seed cũ được giữ lại để bảo toàn lịch sử
         * nhưng không còn tham gia định tuyến.
         */
        foreach (var rule in existingRules)
        {
            if (currentAreaIds.Contains(rule.AreaId))
            {
                continue;
            }

            rule.IsActive = false;
            rule.UpdatedAt = currentTime;
        }

        var rulesByKey =
            existingRules
                .GroupBy(rule => (
                    rule.CategoryId,
                    rule.AreaId,
                    rule.DepartmentId))
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

        foreach (var categorySeed in CategorySeeds)
        {
            var category =
                categories[categorySeed.Name];

            var department =
                departments[
                    categorySeed.DepartmentName];

            foreach (var unitSeed in AdministrativeUnitSeeds)
            {
                var area = areas[unitSeed.Code];

                var key = (
                    CategoryId: category.Id,
                    AreaId: area.Id,
                    DepartmentId: department.Id);

                if (rulesByKey.TryGetValue(
                        key,
                        out var existingRule))
                {
                    existingRule.PriorityOrder = 1;
                    existingRule.IsActive = true;
                    existingRule.UpdatedAt = currentTime;

                    continue;
                }

                var routingRule = new RoutingRule
                {
                    CategoryId = category.Id,
                    AreaId = area.Id,
                    DepartmentId = department.Id,
                    PriorityOrder = 1,
                    IsActive = true,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                };

                _dbContext.RoutingRules.Add(
                    routingRule);

                rulesByKey[key] = routingRule;
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task SeedUsersAsync(
        IReadOnlyDictionary<string, Department> departments,
        DateTime currentTime,
        CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .Where(role =>
                role.Name == RoleNames.Citizen
                || role.Name == RoleNames.Staff
                || role.Name == RoleNames.Admin)
            .ToDictionaryAsync(
                role => role.Name,
                role => role.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        if (!roles.ContainsKey(RoleNames.Citizen)
            || !roles.ContainsKey(RoleNames.Staff)
            || !roles.ContainsKey(RoleNames.Admin))
        {
            throw new InvalidOperationException(
                "Thiếu Role Citizen, Staff hoặc Admin. "
                + "Hãy kiểm tra migration SeedDefaultRoles.");
        }

        var existingEmails =
            await _dbContext.Users
                .AsNoTracking()
                .Select(user => user.Email)
                .ToListAsync(cancellationToken);

        var emailSet =
            existingEmails.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        foreach (var seed in UserSeeds)
        {
            var normalizedEmail =
                seed.Email
                    .Trim()
                    .ToLowerInvariant();

            if (emailSet.Contains(normalizedEmail))
            {
                continue;
            }

            int? departmentId = null;

            if (seed.DepartmentName is not null)
            {
                if (!departments.TryGetValue(
                        seed.DepartmentName,
                        out var department))
                {
                    throw new InvalidOperationException(
                        $"Không tìm thấy Department "
                        + $"'{seed.DepartmentName}'.");
                }

                departmentId = department.Id;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = seed.FullName,
                Email = normalizedEmail,
                PasswordHash =
                    _passwordHasher.Hash(
                        DemoPassword),
                PhoneNumber = seed.PhoneNumber,
                RoleId = roles[seed.RoleName],
                DepartmentId = departmentId,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

            _dbContext.Users.Add(user);

            emailSet.Add(normalizedEmail);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    /*
     * Department trong seed là dữ liệu demo phục vụ
     * nghiệp vụ, không đại diện đầy đủ cơ cấu tổ chức
     * chính thức của Thành phố Hà Nội.
     */
    private static readonly DepartmentSeed[]
        DepartmentSeeds =
        [
            new(
                "Trung tâm Quản lý hạ tầng kỹ thuật Hà Nội",
                "Tiếp nhận sự cố chiếu sáng, công trình "
                + "công cộng và hạ tầng kỹ thuật."),

            new(
                "Ban Duy tu hạ tầng giao thông Hà Nội",
                "Tiếp nhận sự cố mặt đường, vỉa hè "
                + "và hạ tầng giao thông."),

            new(
                "Đơn vị Cấp nước Hà Nội",
                "Tiếp nhận sự cố liên quan đến "
                + "hệ thống cấp nước."),

            new(
                "Công ty Thoát nước Hà Nội",
                "Tiếp nhận sự cố thoát nước, "
                + "ngập úng và cống thoát nước."),

            new(
                "Công ty Môi trường đô thị Hà Nội",
                "Tiếp nhận sự cố rác thải "
                + "và vệ sinh môi trường."),

            new(
                "Đơn vị Quản lý cây xanh Hà Nội",
                "Tiếp nhận sự cố cây xanh đô thị."),

            new(
                "Trung tâm Điều khiển giao thông Hà Nội",
                "Tiếp nhận sự cố biển báo, "
                + "đèn tín hiệu giao thông."),

            new(
                "Trung tâm Hạ tầng số Hà Nội",
                "Tiếp nhận sự cố hạ tầng viễn thông "
                + "và thiết bị đô thị thông minh.")
        ];

    private static readonly CategorySeed[]
        CategorySeeds =
        [
            new(
                "Chiếu sáng đô thị",
                "Đèn đường hỏng, mất sáng, nhấp nháy "
                + "hoặc cột đèn bị hư hỏng.",
                "Trung tâm Quản lý hạ tầng kỹ thuật Hà Nội"),

            new(
                "Mặt đường và vỉa hè",
                "Ổ gà, mặt đường xuống cấp, "
                + "vỉa hè bị hư hỏng hoặc lấn chiếm.",
                "Ban Duy tu hạ tầng giao thông Hà Nội"),

            new(
                "Cấp nước",
                "Rò rỉ đường ống, mất nước "
                + "hoặc áp lực nước không ổn định.",
                "Đơn vị Cấp nước Hà Nội"),

            new(
                "Thoát nước và ngập úng",
                "Cống tắc, nước thải tràn, "
                + "ngập đường hoặc hố ga hư hỏng.",
                "Công ty Thoát nước Hà Nội"),

            new(
                "Rác thải và vệ sinh môi trường",
                "Rác tồn đọng, điểm tập kết sai quy định "
                + "hoặc khu vực mất vệ sinh.",
                "Công ty Môi trường đô thị Hà Nội"),

            new(
                "Cây xanh đô thị",
                "Cây gãy đổ, cành cây nguy hiểm "
                + "hoặc cây cần cắt tỉa.",
                "Đơn vị Quản lý cây xanh Hà Nội"),

            new(
                "Biển báo và đèn tín hiệu giao thông",
                "Biển báo hư hỏng, đèn tín hiệu lỗi "
                + "hoặc thiết bị điều khiển không hoạt động.",
                "Trung tâm Điều khiển giao thông Hà Nội"),

            new(
                "Công trình công cộng",
                "Ghế đá, lan can, nhà vệ sinh công cộng "
                + "hoặc thiết bị công cộng bị hư hỏng.",
                "Trung tâm Quản lý hạ tầng kỹ thuật Hà Nội"),

            new(
                "Hạ tầng viễn thông",
                "Cáp võng thấp, tủ kỹ thuật hư hỏng "
                + "hoặc thiết bị đô thị thông minh gặp lỗi.",
                "Trung tâm Hạ tầng số Hà Nội")
        ];

    private static readonly AdministrativeUnitSeed[]
        AdministrativeUnitSeeds =
        [
            new("Phường Hoàn Kiếm", "HN-W-HOAN-KIEM"),
            new("Phường Cửa Nam", "HN-W-CUA-NAM"),
            new("Phường Ba Đình", "HN-W-BA-DINH"),
            new("Phường Ngọc Hà", "HN-W-NGOC-HA"),
            new("Phường Giảng Võ", "HN-W-GIANG-VO"),
            new("Phường Hai Bà Trưng", "HN-W-HAI-BA-TRUNG"),
            new("Phường Vĩnh Tuy", "HN-W-VINH-TUY"),
            new("Phường Bạch Mai", "HN-W-BACH-MAI"),
            new("Phường Đống Đa", "HN-W-DONG-DA"),
            new("Phường Kim Liên", "HN-W-KIM-LIEN"),
            new("Phường Văn Miếu - Quốc Tử Giám", "HN-W-VAN-MIEU-QUOC-TU-GIAM"),
            new("Phường Láng", "HN-W-LANG"),
            new("Phường Ô Chợ Dừa", "HN-W-O-CHO-DUA"),
            new("Phường Hồng Hà", "HN-W-HONG-HA"),
            new("Phường Lĩnh Nam", "HN-W-LINH-NAM"),
            new("Phường Hoàng Mai", "HN-W-HOANG-MAI"),
            new("Phường Vĩnh Hưng", "HN-W-VINH-HUNG"),
            new("Phường Tương Mai", "HN-W-TUONG-MAI"),
            new("Phường Định Công", "HN-W-DINH-CONG"),
            new("Phường Hoàng Liệt", "HN-W-HOANG-LIET"),
            new("Phường Yên Sở", "HN-W-YEN-SO"),
            new("Phường Thanh Xuân", "HN-W-THANH-XUAN"),
            new("Phường Khương Đình", "HN-W-KHUONG-DINH"),
            new("Phường Phương Liệt", "HN-W-PHUONG-LIET"),
            new("Phường Cầu Giấy", "HN-W-CAU-GIAY"),
            new("Phường Nghĩa Đô", "HN-W-NGHIA-DO"),
            new("Phường Yên Hòa", "HN-W-YEN-HOA"),
            new("Phường Tây Hồ", "HN-W-TAY-HO"),
            new("Phường Phú Thượng", "HN-W-PHU-THUONG"),
            new("Phường Tây Tựu", "HN-W-TAY-TUU"),
            new("Phường Phú Diễn", "HN-W-PHU-DIEN"),
            new("Phường Xuân Đỉnh", "HN-W-XUAN-DINH"),
            new("Phường Đông Ngạc", "HN-W-DONG-NGAC"),
            new("Phường Thượng Cát", "HN-W-THUONG-CAT"),
            new("Phường Từ Liêm", "HN-W-TU-LIEM"),
            new("Phường Xuân Phương", "HN-W-XUAN-PHUONG"),
            new("Phường Tây Mỗ", "HN-W-TAY-MO"),
            new("Phường Đại Mỗ", "HN-W-DAI-MO"),
            new("Phường Long Biên", "HN-W-LONG-BIEN"),
            new("Phường Bồ Đề", "HN-W-BO-DE"),
            new("Phường Việt Hưng", "HN-W-VIET-HUNG"),
            new("Phường Phúc Lợi", "HN-W-PHUC-LOI"),
            new("Phường Hà Đông", "HN-W-HA-DONG"),
            new("Phường Dương Nội", "HN-W-DUONG-NOI"),
            new("Phường Yên Nghĩa", "HN-W-YEN-NGHIA"),
            new("Phường Phú Lương", "HN-W-PHU-LUONG"),
            new("Phường Kiến Hưng", "HN-W-KIEN-HUNG"),
            new("Phường Thanh Liệt", "HN-W-THANH-LIET"),
            new("Phường Chương Mỹ", "HN-W-CHUONG-MY"),
            new("Phường Sơn Tây", "HN-W-SON-TAY"),
            new("Phường Tùng Thiện", "HN-W-TUNG-THIEN"),
            new("Xã Thanh Trì", "HN-C-THANH-TRI"),
            new("Xã Đại Thanh", "HN-C-DAI-THANH"),
            new("Xã Nam Phù", "HN-C-NAM-PHU"),
            new("Xã Ngọc Hồi", "HN-C-NGOC-HOI"),
            new("Xã Thượng Phúc", "HN-C-THUONG-PHUC"),
            new("Xã Thường Tín", "HN-C-THUONG-TIN"),
            new("Xã Chương Dương", "HN-C-CHUONG-DUONG"),
            new("Xã Hồng Vân", "HN-C-HONG-VAN"),
            new("Xã Phú Xuyên", "HN-C-PHU-XUYEN"),
            new("Xã Phượng Dực", "HN-C-PHUONG-DUC"),
            new("Xã Chuyên Mỹ", "HN-C-CHUYEN-MY"),
            new("Xã Đại Xuyên", "HN-C-DAI-XUYEN"),
            new("Xã Thanh Oai", "HN-C-THANH-OAI"),
            new("Xã Bình Minh", "HN-C-BINH-MINH"),
            new("Xã Tam Hưng", "HN-C-TAM-HUNG"),
            new("Xã Dân Hòa", "HN-C-DAN-HOA"),
            new("Xã Vân Đình", "HN-C-VAN-DINH"),
            new("Xã Ứng Thiên", "HN-C-UNG-THIEN"),
            new("Xã Hòa Xá", "HN-C-HOA-XA"),
            new("Xã Ứng Hòa", "HN-C-UNG-HOA"),
            new("Xã Mỹ Đức", "HN-C-MY-DUC"),
            new("Xã Hồng Sơn", "HN-C-HONG-SON"),
            new("Xã Phúc Sơn", "HN-C-PHUC-SON"),
            new("Xã Hương Sơn", "HN-C-HUONG-SON"),
            new("Xã Phú Nghĩa", "HN-C-PHU-NGHIA"),
            new("Xã Xuân Mai", "HN-C-XUAN-MAI"),
            new("Xã Trần Phú", "HN-C-TRAN-PHU"),
            new("Xã Hòa Phú", "HN-C-HOA-PHU"),
            new("Xã Quảng Bị", "HN-C-QUANG-BI"),
            new("Xã Minh Châu", "HN-C-MINH-CHAU"),
            new("Xã Quảng Oai", "HN-C-QUANG-OAI"),
            new("Xã Vật Lại", "HN-C-VAT-LAI"),
            new("Xã Cổ Đô", "HN-C-CO-DO"),
            new("Xã Bất Bạt", "HN-C-BAT-BAT"),
            new("Xã Suối Hai", "HN-C-SUOI-HAI"),
            new("Xã Ba Vì", "HN-C-BA-VI"),
            new("Xã Yên Bài", "HN-C-YEN-BAI"),
            new("Xã Đoài Phương", "HN-C-DOAI-PHUONG"),
            new("Xã Phúc Thọ", "HN-C-PHUC-THO"),
            new("Xã Phúc Lộc", "HN-C-PHUC-LOC"),
            new("Xã Hát Môn", "HN-C-HAT-MON"),
            new("Xã Thạch Thất", "HN-C-THACH-THAT"),
            new("Xã Hạ Bằng", "HN-C-HA-BANG"),
            new("Xã Tây Phương", "HN-C-TAY-PHUONG"),
            new("Xã Hòa Lạc", "HN-C-HOA-LAC"),
            new("Xã Yên Xuân", "HN-C-YEN-XUAN"),
            new("Xã Quốc Oai", "HN-C-QUOC-OAI"),
            new("Xã Hưng Đạo", "HN-C-HUNG-DAO"),
            new("Xã Kiều Phú", "HN-C-KIEU-PHU"),
            new("Xã Phú Cát", "HN-C-PHU-CAT"),
            new("Xã Hoài Đức", "HN-C-HOAI-DUC"),
            new("Xã Dương Hòa", "HN-C-DUONG-HOA"),
            new("Xã Sơn Đồng", "HN-C-SON-DONG"),
            new("Xã An Khánh", "HN-C-AN-KHANH"),
            new("Xã Đan Phượng", "HN-C-DAN-PHUONG"),
            new("Xã Ô Diên", "HN-C-O-DIEN"),
            new("Xã Liên Minh", "HN-C-LIEN-MINH"),
            new("Xã Gia Lâm", "HN-C-GIA-LAM"),
            new("Xã Thuận An", "HN-C-THUAN-AN"),
            new("Xã Bát Tràng", "HN-C-BAT-TRANG"),
            new("Xã Phù Đổng", "HN-C-PHU-DONG"),
            new("Xã Thư Lâm", "HN-C-THU-LAM"),
            new("Xã Đông Anh", "HN-C-DONG-ANH"),
            new("Xã Phúc Thịnh", "HN-C-PHUC-THINH"),
            new("Xã Thiên Lộc", "HN-C-THIEN-LOC"),
            new("Xã Vĩnh Thanh", "HN-C-VINH-THANH"),
            new("Xã Mê Linh", "HN-C-ME-LINH"),
            new("Xã Yên Lãng", "HN-C-YEN-LANG"),
            new("Xã Tiến Thắng", "HN-C-TIEN-THANG"),
            new("Xã Quang Minh", "HN-C-QUANG-MINH"),
            new("Xã Sóc Sơn", "HN-C-SOC-SON"),
            new("Xã Đa Phúc", "HN-C-DA-PHUC"),
            new("Xã Nội Bài", "HN-C-NOI-BAI"),
            new("Xã Trung Giã", "HN-C-TRUNG-GIA"),
            new("Xã Kim Anh", "HN-C-KIM-ANH")
        ];

    private static readonly UserSeed[]
        UserSeeds =
        [
            new(
                "Nguyễn Văn A",
                "nguyenvana@gmail.com",
                "0901000001",
                RoleNames.Admin,
                null),

            new(
                "Công dân Hà Nội 1",
                "citizen.test@gmail.com",
                "0901000002",
                RoleNames.Citizen,
                null),

            new(
                "Công dân Hà Nội 2",
                "citizen.test2@gmail.com",
                "0901000003",
                RoleNames.Citizen,
                null),

            new(
                "Nhân viên Hạ tầng kỹ thuật",
                "staff.dien@gmail.com",
                "0902000001",
                RoleNames.Staff,
                "Trung tâm Quản lý hạ tầng kỹ thuật Hà Nội"),

            new(
                "Nhân viên Giao thông",
                "staff.giaothong@urbanissue.local",
                "0902000002",
                RoleNames.Staff,
                "Ban Duy tu hạ tầng giao thông Hà Nội"),

            new(
                "Nhân viên Cấp nước",
                "staff.capnuoc@urbanissue.local",
                "0902000003",
                RoleNames.Staff,
                "Đơn vị Cấp nước Hà Nội"),

            new(
                "Nhân viên Thoát nước",
                "staff.thoatnuoc@urbanissue.local",
                "0902000004",
                RoleNames.Staff,
                "Công ty Thoát nước Hà Nội"),

            new(
                "Nhân viên Môi trường",
                "staff.moitruong@urbanissue.local",
                "0902000005",
                RoleNames.Staff,
                "Công ty Môi trường đô thị Hà Nội"),

            new(
                "Nhân viên Cây xanh",
                "staff.cayxanh@urbanissue.local",
                "0902000006",
                RoleNames.Staff,
                "Đơn vị Quản lý cây xanh Hà Nội"),

            new(
                "Nhân viên Điều khiển giao thông",
                "staff.tinnghieu@urbanissue.local",
                "0902000007",
                RoleNames.Staff,
                "Trung tâm Điều khiển giao thông Hà Nội"),

            new(
                "Nhân viên Hạ tầng số",
                "staff.hatangso@urbanissue.local",
                "0902000008",
                RoleNames.Staff,
                "Trung tâm Hạ tầng số Hà Nội")
        ];

    private sealed record CategorySeed(
        string Name,
        string Description,
        string DepartmentName);

    private sealed record AdministrativeUnitSeed(
        string Name,
        string Code);

    private sealed record DepartmentSeed(
        string Name,
        string Description);

    private sealed record UserSeed(
        string FullName,
        string Email,
        string? PhoneNumber,
        string RoleName,
        string? DepartmentName);
}
