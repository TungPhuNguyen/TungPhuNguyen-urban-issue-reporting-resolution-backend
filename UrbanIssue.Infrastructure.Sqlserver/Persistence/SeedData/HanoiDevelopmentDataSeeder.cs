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

        /*
         * Dictionary được trả về theo Code để phân biệt rõ:
         *
         * Quận Ba Đình  -> HN-D-BA-DINH
         * Phường Ba Đình -> HN-W-BA-DINH
         */
        var areasByCode =
            existingAreas
                .Where(area =>
                    !string.IsNullOrWhiteSpace(area.Code))
                .ToDictionary(
                    area => area.Code!,
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
         * Bước 1: tạo các Quận.
         * Quận là node gốc nên ParentAreaId luôn bằng null.
         */
        foreach (var districtSeed in DistrictSeeds)
        {
            if (!areasByCode.TryGetValue(
                    districtSeed.Code,
                    out var district)
                && !areasByName.TryGetValue(
                    districtSeed.Name,
                    out district))
            {
                district = new Area
                {
                    Name = districtSeed.Name,
                    Code = districtSeed.Code,
                    ParentAreaId = null,
                    IsActive = true,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                };

                _dbContext.Areas.Add(district);
            }
            else
            {
                district.Name = districtSeed.Name;
                district.Code = districtSeed.Code;
                district.ParentAreaId = null;
                district.IsActive = true;
                district.UpdatedAt = currentTime;
            }

            areasByCode[districtSeed.Code] =
                district;

            areasByName[districtSeed.Name] =
                district;
        }

        /*
         * Save trước để các Quận mới có Id,
         * sau đó mới gán ParentAreaId cho Phường.
         */
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        /*
         * Bước 2: tạo các Phường và gán Quận cha.
         *
         * Seeder cũng nhận diện dữ liệu cũ như "Ba Đình",
         * "Ngọc Hà"... để chuyển thành node Phường thay vì
         * tạo bản ghi trùng khi seeder phiên bản cũ đã chạy.
         */
        foreach (var districtSeed in DistrictSeeds)
        {
            var district =
                areasByCode[districtSeed.Code];

            foreach (var wardSeed in districtSeed.Wards)
            {
                var legacyWardName =
                    wardSeed.Name.StartsWith(
                        "Phường ",
                        StringComparison.OrdinalIgnoreCase)
                        ? wardSeed.Name["Phường ".Length..]
                        : wardSeed.Name;

                if (!areasByCode.TryGetValue(
                        wardSeed.Code,
                        out var ward)
                    && !areasByName.TryGetValue(
                        wardSeed.Name,
                        out ward)
                    && !areasByName.TryGetValue(
                        legacyWardName,
                        out ward))
                {
                    ward = new Area
                    {
                        Name = wardSeed.Name,
                        Code = wardSeed.Code,
                        ParentAreaId = district.Id,
                        IsActive = true,
                        CreatedAt = currentTime,
                        UpdatedAt = null
                    };

                    _dbContext.Areas.Add(ward);
                }
                else
                {
                    ward.Name = wardSeed.Name;
                    ward.Code = wardSeed.Code;
                    ward.ParentAreaId = district.Id;
                    ward.IsActive = true;
                    ward.UpdatedAt = currentTime;
                }

                areasByCode[wardSeed.Code] =
                    ward;

                areasByName[wardSeed.Name] =
                    ward;
            }
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
                .AsNoTracking()
                .Select(rule => new
                {
                    rule.CategoryId,
                    rule.AreaId,
                    rule.DepartmentId
                })
                .ToListAsync(cancellationToken);

        var existingKeys =
            existingRules
                .Select(rule => (
                    rule.CategoryId,
                    rule.AreaId,
                    rule.DepartmentId))
                .ToHashSet();

        /*
         * Routing Rule chỉ được tạo cho Phường.
         * Quận chỉ đóng vai trò node cha để frontend
         * hiển thị và lọc cây khu vực.
         */
        foreach (var categorySeed in CategorySeeds)
        {
            var category =
                categories[categorySeed.Name];

            var department =
                departments[
                    categorySeed.DepartmentName];

            foreach (var districtSeed in DistrictSeeds)
            {
                foreach (var wardSeed in districtSeed.Wards)
                {
                    var area =
                        areas[wardSeed.Code];

                    var key = (
                        CategoryId: category.Id,
                        AreaId: area.Id,
                        DepartmentId: department.Id);

                    if (existingKeys.Contains(key))
                    {
                        continue;
                    }

                    _dbContext.RoutingRules.Add(
                        new RoutingRule
                        {
                            CategoryId = category.Id,
                            AreaId = area.Id,
                            DepartmentId = department.Id,
                            PriorityOrder = 1,
                            IsActive = true,
                            CreatedAt = currentTime,
                            UpdatedAt = null
                        });

                    existingKeys.Add(key);
                }
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

    private static readonly DistrictSeed[]
        DistrictSeeds =
        [
            new(
                "Quận Ba Đình",
                "HN-D-BA-DINH",
                [
                    new("Phường Ba Đình", "HN-W-BA-DINH"),
                    new("Phường Ngọc Hà", "HN-W-NGOC-HA"),
                    new("Phường Giảng Võ", "HN-W-GIANG-VO")
                ]),

            new(
                "Quận Hoàn Kiếm",
                "HN-D-HOAN-KIEM",
                [
                    new("Phường Hoàn Kiếm", "HN-W-HOAN-KIEM"),
                    new("Phường Cửa Nam", "HN-W-CUA-NAM")
                ]),

            new(
                "Quận Hai Bà Trưng",
                "HN-D-HAI-BA-TRUNG",
                [
                    new(
                        "Phường Hai Bà Trưng",
                        "HN-W-HAI-BA-TRUNG"),

                    new("Phường Bạch Mai", "HN-W-BACH-MAI"),
                    new("Phường Vĩnh Tuy", "HN-W-VINH-TUY")
                ]),

            new(
                "Quận Đống Đa",
                "HN-D-DONG-DA",
                [
                    new("Phường Đống Đa", "HN-W-DONG-DA"),
                    new("Phường Láng", "HN-W-LANG"),
                    new("Phường Ô Chợ Dừa", "HN-W-O-CHO-DUA"),
                    new("Phường Kim Liên", "HN-W-KIM-LIEN"),

                    new(
                        "Phường Văn Miếu - Quốc Tử Giám",
                        "HN-W-VAN-MIEU-QUOC-TU-GIAM")
                ]),

            new(
                "Quận Thanh Xuân",
                "HN-D-THANH-XUAN",
                [
                    new(
                        "Phường Thanh Xuân",
                        "HN-W-THANH-XUAN"),

                    new(
                        "Phường Khương Đình",
                        "HN-W-KHUONG-DINH"),

                    new(
                        "Phường Phương Liệt",
                        "HN-W-PHUONG-LIET")
                ]),

            new(
                "Quận Cầu Giấy",
                "HN-D-CAU-GIAY",
                [
                    new("Phường Cầu Giấy", "HN-W-CAU-GIAY"),
                    new("Phường Nghĩa Đô", "HN-W-NGHIA-DO"),
                    new("Phường Yên Hòa", "HN-W-YEN-HOA")
                ]),

            new(
                "Quận Tây Hồ",
                "HN-D-TAY-HO",
                [
                    new("Phường Tây Hồ", "HN-W-TAY-HO"),
                    new("Phường Hồng Hà", "HN-W-HONG-HA"),
                    new("Phường Phú Thượng", "HN-W-PHU-THUONG")
                ]),

            new(
                "Quận Hoàng Mai",
                "HN-D-HOANG-MAI",
                [
                    new("Phường Định Công", "HN-W-DINH-CONG"),
                    new("Phường Hoàng Liệt", "HN-W-HOANG-LIET"),
                    new("Phường Tương Mai", "HN-W-TUONG-MAI"),
                    new("Phường Hoàng Mai", "HN-W-HOANG-MAI"),
                    new("Phường Yên Sở", "HN-W-YEN-SO"),
                    new("Phường Vĩnh Hưng", "HN-W-VINH-HUNG"),
                    new("Phường Lĩnh Nam", "HN-W-LINH-NAM")
                ]),

            new(
                "Quận Long Biên",
                "HN-D-LONG-BIEN",
                [
                    new("Phường Việt Hưng", "HN-W-VIET-HUNG"),
                    new("Phường Bồ Đề", "HN-W-BO-DE"),
                    new("Phường Long Biên", "HN-W-LONG-BIEN"),
                    new("Phường Phúc Lợi", "HN-W-PHUC-LOI")
                ]),

            new(
                "Quận Hà Đông",
                "HN-D-HA-DONG",
                [
                    new("Phường Hà Đông", "HN-W-HA-DONG"),
                    new("Phường Dương Nội", "HN-W-DUONG-NOI"),
                    new("Phường Yên Nghĩa", "HN-W-YEN-NGHIA"),
                    new("Phường Kiến Hưng", "HN-W-KIEN-HUNG"),
                    new("Phường Phú Lương", "HN-W-PHU-LUONG")
                ]),

            new(
                "Quận Bắc Từ Liêm",
                "HN-D-BAC-TU-LIEM",
                [
                    new("Phường Tây Tựu", "HN-W-TAY-TUU"),
                    new("Phường Phú Diễn", "HN-W-PHU-DIEN"),
                    new("Phường Xuân Đỉnh", "HN-W-XUAN-DINH"),
                    new("Phường Đông Ngạc", "HN-W-DONG-NGAC"),
                    new("Phường Thượng Cát", "HN-W-THUONG-CAT")
                ]),

            new(
                "Quận Nam Từ Liêm",
                "HN-D-NAM-TU-LIEM",
                [
                    new("Phường Từ Liêm", "HN-W-TU-LIEM"),
                    new("Phường Tây Mỗ", "HN-W-TAY-MO"),
                    new("Phường Đại Mỗ", "HN-W-DAI-MO"),
                    new(
                        "Phường Xuân Phương",
                        "HN-W-XUAN-PHUONG")
                ])
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

    private sealed record DistrictSeed(
        string Name,
        string Code,
        IReadOnlyList<WardSeed> Wards);

    private sealed record WardSeed(
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
