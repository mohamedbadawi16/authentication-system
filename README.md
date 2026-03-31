# authentication-system

نظام مصادقة كامل (Backend + Frontend) مبني بـ ASP.NET Core.  
يشمل تسجيل، تسجيل دخول، تأكيد بريد عبر OTP، وإعادة إرسال OTP، مع Clean Architecture وفصل طبقات واضح.

## المميزات
- Register / Login / Verify OTP / Resend OTP
- JWT Authentication
- تشفير كلمات المرور باستخدام `PasswordHasher`
- OTP مؤقت مع انتهاء صلاحية
- Rate Limiting لطلبات OTP
- Validation + Error Handling + Logging
- MVC UI بسيطة بـ Bootstrap

## المعمارية
- `src/Domain` الكيانات الأساسية
- `src/Application` الـ Use Cases والخدمات والمنطق
- `src/Infrastructure` التخزين، JWT، OTP، البريد
- `src/Api` Web API
- `src/Web` MVC Frontend

## التشغيل السريع
1. شغل الـ API:
   ```bash
   dotnet run --project src/Api
   ```
2. شغل الـ MVC:
   ```bash
   dotnet run --project src/Web
   ```

## الإعدادات
### قاعدة البيانات (Local SQL Server)
الكونكشن الافتراضي في `src/Api/appsettings.json`:
```json
"Default": "Server=(localdb)\\MSSQLLocalDB;Database=AuthenticationSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### JWT
لازم `SigningKey` يكون 32 بايت على الأقل:
```json
"Jwt": {
  "Issuer": "AuthenticationSystem",
  "Audience": "AuthenticationSystem",
  "SigningKey": "dev-only-change-me-please-32bytes-minimum-key-123456"
}
```

### OTP
حاليًا مضبوط على كود ثابت للتجارب فقط:
```json
"Otp": {
  "CodeLength": 6,
  "ExpiryMinutes": 10,
  "MinResendIntervalSeconds": 60,
  "MaxPerHour": 5,
  "HashKey": "replace-with-otp-hash-key",
  "FixedCode": "123456"
}
```
> احذف `FixedCode` في الإنتاج.

## ملاحظات
- الـ OTP بيظهر في لوج الـ API عبر `LoggingEmailSender` (لتجارب التطوير).
- الـ Frontend بيخزن JWT في Session (للعرض فقط).

## الخطوات القادمة المقترحة
- استبدال `LoggingEmailSender` بمزوّد بريد حقيقي.
- إضافة Refresh Tokens.
- حماية الـ Session وتخزين التوكن بشكل آمن.
