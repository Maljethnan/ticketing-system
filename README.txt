===============================================================================
نظام تتبع المشكلات التقنية — هيئة الرقابة ومكافحة الفساد (نزاها)
الإصدار 2.0 | تاريخ التحديث: 2026-10-08
===============================================================================

هذا المشروع هو نظام متكامل لإدارة وتتبع المشكلات التقنية الداخلية داخل المؤسسة،
ويعتمد على بنية ناعمة (Clean Architecture) لتفكيك المسؤوليات بين طبقات التطبيق:
الكيانات، الوصول للبيانات، الخدمات، واجهة API، والواجهة الويب.

-------------------------------------------------------------------------------
1) هدف النظام
-------------------------------------------------------------------------------
- تسجيل المشكلات التقنية وطلب المساعدة
- توزيع التذكرة بين موظفي الدعم والجهات المعنية
- متابعة حالة التذكرة من الفتح وحتى الإغلاق
- تطبيق صلاحيات حسب الدور والهيكل الإداري
- دعم التقارير الإحصائية ومؤشرات SLA
- دعم الاتصال الفوري عبر SignalR
- دعم مصادقة JWT وAD (Active Directory)

-------------------------------------------------------------------------------
2) هيكل المشروع
-------------------------------------------------------------------------------
TicketingSystem/
├── TicketingSystem.Core/                 ← طبقة النواة / Business Domain
│   ├── Entities/                         ← النماذج الأساسية والكيانات
│   ├── DTOs/                             ← كائنات نقل البيانات
│   ├── Interfaces/                       ← مواصفات الخدمات والواجهات
│   └── Services/                         ← منطق عام/خدمات أساسية إن وجدت
├── TicketingSystem.Data/                 ← طبقة قاعدة البيانات والوصول للبيانات
│   ├── Context/                          ← AppDbContext
│   ├── Repositories/                     ← Repository pattern
│   └── Seeders/                          ← بيانات أولية وتهيئة النظام
├── TicketingSystem.API/                  ← طبقة خدمات REST API
│   ├── Controllers/                      ← نقاط النهاية (Auth, Tickets, Comments...)
│   ├── Services/                         ← خدمات الأعمال (AuthService, TicketService...)
│   ├── Middleware/                       ← ErrorHandling, RequestLogging
│   ├── Hubs/                             ← NotificationHub / SignalR
│   ├── Configuration/                    ← إعدادات إضافية
│   ├── Program.cs                       ← نقطة بدء التطبيق
│   └── appsettings.json                 ← إعدادات التطبيق
├── TicketingSystem.Web/                  ← واجهة المستخدم (Blazor Server)
│   ├── Pages/                            ← الصفحات والواجهات
│   ├── Services/                         ← خدمـات العميل
│   ├── Components/                      ← مكونات قابلة لإعادة الاستخدام
│   └── wwwroot/                         ← الأصول الثابتة
├── Database/                             ← ملفات SQL للقاعدة
│   └── Setup_Database.sql               ← سكربت إنشاء قاعدة البيانات
├── Installation/                         ← أدلة التثبيت والمتطلبات
│   ├── 01_متطلبات_البيئة.txt
│   ├── 02_دليل_التنصيب_الخطوة_بخطوة.txt
│   ├── 03_دليل_الأمان_والتشفير.txt
│   ├── 04_دليل_استكشاف_الأخطاء.txt
│   └── 05_وصف_المكونات_ومخطط_العلاقات.txt
├── Architecture_Overview.txt             ← نظرة عامة على البنية المعمارية
├── System_Relationships.txt              ← العلاقات بين المكونات والكيانات
├── README.txt                            ← هذا الملف
├── docker-compose.yml                    ← إعدادات التشغيل المحلي
├── TicketingSystem.slnx                  ← ملف الحل الرئيسي
├── .env                                  ← متغيرات بيئة محلية
└── sql-init                              ← ملفات إعداد قاعدة البيانات الأولية

-------------------------------------------------------------------------------
3) طبقات المعمارية الرئيسية
-------------------------------------------------------------------------------
3.1 TicketingSystem.Core
- يحتوي على الكيانات الأساسية مثل User، Role، Ticket، TicketComment، TicketAttachment
- يحتوي على DTOs مثل CreateTicketDto، AuthResponseDto، FilterRequest، Request DTOs
- يحتوي على الواجهات مثل IAuthService، ITicketService، IPermissionService
- لا يعتمد على طبقة API أو Data مباشرة، ويُعد قلب النظام

3.2 TicketingSystem.Data
- يضم DbContext الأساسي AppDbContext
- يعرّف DbSet لكل كيان أساسي
- يطبق إعدادات العلاقات، المفاتيح الأجنبية، الفهارس، والبيانات الأولية عبر SeedData
- يعمل كطبقة الوصول إلى SQL Server

3.3 TicketingSystem.API
- نقطة الاتصال الخارجية للنظام عبر REST API
- يدير Authentication / Authorization
- يحتوي على Controllers مثل AuthController، TicketsController، CommentsController، AttachmentsController، ReportsController
- يوجد فيه خدمات الأعمال مثل AuthService، TicketService، PermissionService، NotificationService
- يدعم JWT، SignalR، Middleware، وملفات الرفع / الحفظ

3.4 TicketingSystem.Web
- واجهة المستخدم باستخدام Blazor Server
- صفحات مثل Login، Dashboard، Tickets، Detail، Reports
- خدمات العميل للتفاعل مع API
- ينفذ طبقة العرض فقط دون احتواء منطق الأعمال الرئيسي

-------------------------------------------------------------------------------
4) المسارات المعتمدة في المشروع
-------------------------------------------------------------------------------
الاعتماد الحقيقي بين المكونات يكون على النحو التالي:

TicketingSystem.Web
    ↓ calls API
TicketingSystem.API
    ↓ uses
TicketingSystem.Core (Entities, DTOs, Interfaces)
    ↓ uses
TicketingSystem.Data (DbContext + EF Core)
    ↓ uses
SQL Server

ملاحظة:
- TicketingSystem.API يعتمد على Core وData
- TicketingSystem.Web يعتمد على API مباشرة ولا يعتمد مباشرة على Data
- Core لا يعتمد على أي طبقة أخرى

-------------------------------------------------------------------------------
5) العلاقات الأساسية بين الكيانات
-------------------------------------------------------------------------------
1. المستخدمين والأدوار
- User -> Role
- User <-> UserRole -> Role
- User -> GeneralDepartment
- User -> UserDepartmentAccess -> SubDepartment

2. الهيكل الإداري
- GeneralDepartment يحتوي عدد من SubDepartment
- SubDepartment ينتمي إلى GeneralDepartment
- UserDepartmentAccess يمنح صلاحية مستخدم على قسم فرعي محدد

3. نظام التذاكر
- Ticket -> SystemEntity
- Ticket -> IssueType
- Ticket -> Priority
- Ticket -> TicketStatus
- Ticket -> User (Creator and AssignedToUser)
- Ticket -> TicketComment
- Ticket -> TicketAttachment
- Ticket -> TicketAccessGrant
- Ticket -> TicketStatusHistory

4. التعليقات والمرفقات
- TicketComment -> Ticket
- TicketComment -> User (Sender)
- TicketComment -> TicketComment (ParentComment / Replies)
- TicketAttachment -> Ticket
- TicketAttachment -> TicketComment

5. التصعيد
- EscalationRule -> Priority
- EscalationLog -> Ticket / User
- AuditLog -> User

6. المصادقة والأمان
- JWT يوقّع الرمز ويعتمده API
- Active Directory (LDAP) يتم التحقق منه في AuthService
- كلمات المرور المحلية تُخزن باستخدام BCrypt

-------------------------------------------------------------------------------
6) مخطط العلاقات المعروفة في DbContext
-------------------------------------------------------------------------------
AppDbContext يعرّف DbSet للكيانات التالية:
- Users
- Roles
- UserRoles
- RolePermissions
- GeneralDepartments
- SubDepartments
- UserDepartmentAccesses
- Systems
- SystemSpecialists
- IssueTypes
- Priorities
- TicketStatuses
- Tickets
- TicketStatusHistories
- TicketComments
- TicketAttachments
- TicketAccessGrants
- EscalationRules
- EscalationLogs
- AuditLogs
- KnowledgeBases

العلاقات الأساسية المكوّنة في OnModelCreating:
- User -> GeneralDepartment (Restrict)
- UserRole -> User / Role
- Ticket -> Creator / AssignedToUser / System
- TicketComment -> Ticket / ParentComment
- TicketAttachment -> Ticket / Comment
- EscalationRule (PriorityId + StepNumber) unique index
- TicketNumber unique index
- Username وEmail unique indexes

-------------------------------------------------------------------------------
7) سير العمل الرئيسي للنظام
-------------------------------------------------------------------------------
7.1 تسجيل الدخول
1. المستخدم يرسل اسم المستخدم وكلمة المرور إلى AuthController
2. AuthService يتحقق من القيم عبر LDAP أو قاعدة البيانات المحلية
3. إذا نجح، يتم إنشاء JWT أو إرجاع بيانات المستخدم
4. العميل يستخدم الرمز في كل طلب محمي

7.2 إنشاء تذكرة
1. المستخدم يرسل بيانات التذكرة إلى TicketsController
2. TicketService يحقّق البيانات ويحدد الأولوية وقسم المستخدم
3. يتم إنشاء Ticket وحفظه في قاعدة البيانات
4. يتم حفظ رقم التذكرة، الحالة الأولية، والمرجع إلى النظام والقسم

7.3 تحديث الحالة
1. يتم نقل الطلب إلى TicketService
2. يتم تحديث StatusId في قاعدة البيانات
3. يتم تسجيل التأثير في سجل الحالة أو في السجلات ذات الصلة

7.4 التعليق وإرفاق الملفات
1. CommentsController يضيف تعليقاً جديداً على Ticket
2. AttachmentsController يرفع ملفاً ويضيفه إلى Ticket أو Comment
3. يتم توثيق الحدث في AuditLog وNotificationService

7.5 التقرير
1. ReportsController يجمع البيانات من Tickets وUsers وSystems
2. يحسب حالات الفتح، الإغلاق، SLA، التوزيع حسب الأولوية/النظام/الإدارة
3. يُرجع JSON مناسب للواجهة أو للتصدير

-------------------------------------------------------------------------------
8) الأمان والامتثال
-------------------------------------------------------------------------------
- JWTBearer مستخدم في API للتحقق من التوكن
- SecretKey يتم قراءته من appsettings.json أو متغيرات البيئة
- كلمات المرور تُخزن عبر BCrypt
- Active Directory LDAP مدعوم لتسجيل الدخول داخل المؤسسة
- عمليات الحذف لا تُزال فعلياً من قاعدة البيانات في بعض الحالات؛ تُستخدم علامات DeletedAt وتاريخ الحذف بدل الحذف الجسدي

-------------------------------------------------------------------------------
9) القيم الافتراضية الأولية
-------------------------------------------------------------------------------
اسم المستخدم الافتراضي: admin
كلمة المرور الافتراضية: Admin@2025!

يُستحسن تغيير كلمة المرور فوراً بعد أول دخول، وتغيير SecretKey في بيئة الإنتاج.

-------------------------------------------------------------------------------
10) أوامر التشغيل السريعة
-------------------------------------------------------------------------------
يُبنى المشروع ويُشغّل بالكامل داخل حاويات Docker؛ لا حاجة لتثبيت .NET أو تشغيل
أوامر dotnet على الجهاز المضيف:

1. docker compose up --build -d
2. الواجهة: http://localhost:5000
3. API: http://localhost:8080
4. متابعة السجلات: docker compose logs -f api web db
5. إيقاف الخدمات مع الاحتفاظ ببيانات قاعدة البيانات: docker compose down

تسجيل الدخول الأولي: admin / Admin@2025! — غيّر كلمة المرور بعد أول دخول.

-------------------------------------------------------------------------------
11) ملاحظات تطويرية
-------------------------------------------------------------------------------
- هذا المشروع في مرحلته الحالية يعمل على ASP.NET Core 8
- يتم استخدام EF Core مع SQL Server
- الوحدة الأساسية هي Ticket، ويتم بناء بقية التفاعل حولها
- النظام مصمم ليكون قابلاً للتوسع بإضافة وحدات مثل إدارة الموظفين، تقارير متقدمة، أو أتمتة تشغيلية

===============================================================================
نهاية الملف
===============================================================================
