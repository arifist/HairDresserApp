using System.Text.Json;
using System.Text.Json.Serialization;
using BarberBookingApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BarberBookingApp.Data;

public class JsonDataStore
{
    private readonly string _dataDir;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _canWrite;

    private List<Customer> _customers = new();
    private List<Admin> _admins = new();
    private List<ServiceType> _serviceTypes = new();
    private List<WorkingHour> _workingHours = new();
    private List<Appointment> _appointments = new();
    private List<AppointmentServiceItem> _appointmentServiceItems = new();
    private List<OtpVerification> _otpVerifications = new();
    private List<SmsLog> _smsLogs = new();
    private List<ContactMessage> _contactMessages = new();

    public IReadOnlyList<Customer> Customers => _customers;
    public IReadOnlyList<Admin> Admins => _admins;
    public IReadOnlyList<ServiceType> ServiceTypes => _serviceTypes;
    public IReadOnlyList<WorkingHour> WorkingHours => _workingHours;
    public IReadOnlyList<Appointment> Appointments => _appointments;
    public IReadOnlyList<AppointmentServiceItem> AppointmentServiceItems => _appointmentServiceItems;
    public IReadOnlyList<OtpVerification> OtpVerifications => _otpVerifications;
    public IReadOnlyList<SmsLog> SmsLogs => _smsLogs;
    public IReadOnlyList<ContactMessage> ContactMessages => _contactMessages;

    public JsonDataStore(IWebHostEnvironment env, IConfiguration config)
    {
        var dataDir = config["Database:JsonDataDir"] ?? "App_Data";
        _dataDir = Path.Combine(env.ContentRootPath, dataDir);

        try
        {
            Directory.CreateDirectory(_dataDir);
            _canWrite = true;
        }
        catch
        {
            _canWrite = false;
        }

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new TimeSpanJsonConverter(), new JsonStringEnumConverter() }
        };

        Load();
        SeedIfEmpty();
    }

    private void Load()
    {
        _customers = LoadFile<Customer>("customers.json");
        _admins = LoadFile<Admin>("admins.json");
        _serviceTypes = LoadFile<ServiceType>("service_types.json");
        _workingHours = LoadFile<WorkingHour>("working_hours.json");
        _appointments = LoadFile<Appointment>("appointments.json");
        _appointmentServiceItems = LoadFile<AppointmentServiceItem>("appointment_service_items.json");
        _otpVerifications = LoadFile<OtpVerification>("otp_verifications.json");
        _smsLogs = LoadFile<SmsLog>("sms_logs.json");
        _contactMessages = LoadFile<ContactMessage>("contact_messages.json");
        PopulateNavProps();
    }

    private List<T> LoadFile<T>(string fileName)
    {
        var path = Path.Combine(_dataDir, fileName);
        if (!File.Exists(path)) return new();
        try { return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), _jsonOptions) ?? new(); }
        catch { return new(); }
    }

    private void PopulateNavProps()
    {
        var custMap = _customers.ToDictionary(c => c.Id);
        var stMap = _serviceTypes.ToDictionary(s => s.Id);
        var apptMap = _appointments.ToDictionary(a => a.Id);

        foreach (var c in _customers) c.Appointments = new();
        foreach (var a in _appointments)
        {
            a.ServiceItems = new();
            if (custMap.TryGetValue(a.CustomerId, out var c)) { a.Customer = c; c.Appointments.Add(a); }
            if (stMap.TryGetValue(a.ServiceTypeId, out var st)) a.ServiceType = st;
        }
        foreach (var item in _appointmentServiceItems)
        {
            if (apptMap.TryGetValue(item.AppointmentId, out var a)) { item.Appointment = a; a.ServiceItems.Add(item); }
            if (stMap.TryGetValue(item.ServiceTypeId, out var st)) item.ServiceType = st;
        }
    }

    private int NextId<T>(List<T> list, Func<T, int> idSelector)
        => list.Count > 0 ? list.Max(idSelector) + 1 : 1;

    public Task<Customer?> FindCustomerByPhoneAsync(string phone)
        => Task.FromResult(_customers.FirstOrDefault(c => c.PhoneNumber == phone));

    public Task<Customer?> FindCustomerAsync(int id)
        => Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public async Task AddCustomerAsync(Customer customer)
    {
        customer.Id = NextId(_customers, c => c.Id);
        customer.Appointments = new();
        _customers.Add(customer);
        await SaveAsync("customers.json", _customers);
    }

    public Task UpdateCustomerAsync() => SaveAsync("customers.json", _customers);

    public Task<Admin?> FindAdminByUsernameAsync(string username)
        => Task.FromResult(_admins.FirstOrDefault(a => a.Username == username));

    public async Task AddAppointmentAsync(Appointment appt)
    {
        appt.Id = NextId(_appointments, a => a.Id);
        appt.ServiceItems = new();
        var st = _serviceTypes.FirstOrDefault(s => s.Id == appt.ServiceTypeId);
        appt.ServiceType = st;
        if (appt.Customer is not null) appt.Customer.Appointments.Add(appt);
        _appointments.Add(appt);
        await SaveAsync("appointments.json", _appointments);
    }

    public Task UpdateAppointmentAsync() => SaveAsync("appointments.json", _appointments);

    public async Task AddAppointmentServiceItemsAsync(IEnumerable<AppointmentServiceItem> items)
    {
        foreach (var item in items)
        {
            item.Id = NextId(_appointmentServiceItems, i => i.Id);
            var appt = _appointments.FirstOrDefault(a => a.Id == item.AppointmentId);
            var st = _serviceTypes.FirstOrDefault(s => s.Id == item.ServiceTypeId);
            item.Appointment = appt;
            item.ServiceType = st;
            appt?.ServiceItems.Add(item);
            _appointmentServiceItems.Add(item);
        }
        await SaveAsync("appointment_service_items.json", _appointmentServiceItems);
    }

    public async Task AddServiceTypeAsync(ServiceType st)
    {
        st.Id = NextId(_serviceTypes, s => s.Id);
        _serviceTypes.Add(st);
        await SaveAsync("service_types.json", _serviceTypes);
    }

    public Task UpdateServiceTypesAsync() => SaveAsync("service_types.json", _serviceTypes);

    public Task UpdateWorkingHoursAsync() => SaveAsync("working_hours.json", _workingHours);

    public async Task AddOtpVerificationAsync(OtpVerification otp)
    {
        otp.Id = NextId(_otpVerifications, o => o.Id);
        _otpVerifications.Add(otp);
        await SaveAsync("otp_verifications.json", _otpVerifications);
    }

    public Task UpdateOtpVerificationsAsync() => SaveAsync("otp_verifications.json", _otpVerifications);

    public async Task AddSmsLogAsync(SmsLog log)
    {
        log.Id = NextId(_smsLogs, l => l.Id);
        _smsLogs.Add(log);
        await SaveAsync("sms_logs.json", _smsLogs);
    }

    public async Task AddContactMessageAsync(ContactMessage msg)
    {
        msg.Id = NextId(_contactMessages, m => m.Id);
        _contactMessages.Add(msg);
        await SaveAsync("contact_messages.json", _contactMessages);
    }

    public Task UpdateContactMessagesAsync() => SaveAsync("contact_messages.json", _contactMessages);

    private async Task SaveAsync<T>(string fileName, List<T> data)
    {
        if (!_canWrite) return;
        await _saveLock.WaitAsync();
        try
        {
            var path = Path.Combine(_dataDir, fileName);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(data, _jsonOptions));
        }
        catch
        {
            _canWrite = false;
        }
        finally { _saveLock.Release(); }
    }

    private void SeedIfEmpty()
    {
        if (!_admins.Any())
        {
            var admin = new Admin { Id = 1, Username = "arif", DisplayName = "Arif" };
            admin.PasswordHash = new PasswordHasher<Admin>().HashPassword(admin, "Arif2026!");
            _admins.Add(admin);
            TrySaveSync("admins.json", _admins);
        }
        if (!_serviceTypes.Any())
        {
            _serviceTypes.AddRange(new[]
            {
                new ServiceType { Id = 1, Name = "Saç Kesimi", Description = "Yüz şekline ve saç yapısına uygun, makas ve tıraş makinesi ile profesyonel saç kesimi.", DurationMinutes = 30, Price = 250, SortOrder = 1, Icon = "bi-scissors" },
                new ServiceType { Id = 2, Name = "Sakal Tıraşı", Description = "Sıcak havlu ve özel jilet bakımı ile şekillendirme dahil sakal tıraşı.", DurationMinutes = 30, Price = 200, SortOrder = 2, Icon = "bi-droplet" },
                new ServiceType { Id = 3, Name = "Saç + Sakal", Description = "Saç kesimi ve sakal tıraşının birlikte uygulandığı ekonomik bakım paketi.", DurationMinutes = 60, Price = 400, SortOrder = 3, Icon = "bi-stars" },
                new ServiceType { Id = 4, Name = "Diğer", Description = "Yukarıdaki seçeneklere uymayan kısa danışma/bakım talepleri için.", DurationMinutes = 30, Price = 0, SortOrder = 4, Icon = "bi-three-dots" }
            });
            TrySaveSync("service_types.json", _serviceTypes);
        }
        if (!_workingHours.Any())
        {
            int id = 1;
            foreach (var day in Enum.GetValues<DayOfWeek>())
                _workingHours.Add(new WorkingHour { Id = id++, DayOfWeek = day, IsOpen = day != DayOfWeek.Sunday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(22, 0, 0) });
            TrySaveSync("working_hours.json", _workingHours);
        }
    }

    private void TrySaveSync<T>(string fileName, List<T> data)
    {
        if (!_canWrite) return;
        try
        {
            var path = Path.Combine(_dataDir, fileName);
            File.WriteAllText(path, JsonSerializer.Serialize(data, _jsonOptions));
        }
        catch
        {
            _canWrite = false;
        }
    }
}

public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.Parse(reader.GetString()!);
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
}
