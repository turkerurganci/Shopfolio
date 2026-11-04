using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <summary>
    /// The start-up data of an installation (T119; 06 8.1.1): the 81 provinces with their plate codes, the country list, the six single rows -
    /// <c>Company</c>, <c>CompanySettings</c>, <c>Branding</c>, <c>ChannelStateRecord</c>, <c>HeartbeatRecord</c>, <c>BackupStatusRecord</c> -
    /// with their start values, the 81 delivery provinces (the whole of Turkey) and the two drafts of the firm-edited legal texts
    /// (<c>PrivacyNotice</c>, <c>CookiePolicy</c>) without a publish time or a version number. The application has no "create it when it is
    /// missing" path: these rows exist because this migration wrote them.
    /// <para>
    /// The statements are fixed: the ids are version 7 UUIDs built from the migration's own moment and a counter, the creation time is that
    /// moment (the migration reads no clock; 09 7.1, 10.6 item 11), and every statement only inserts what is not there yet. Applying the data a
    /// second time therefore changes neither a row count nor a value the company has changed (06 8.1.2): a list row is matched by its code, a
    /// single row by its single-row index, a draft by its type, and the delivery provinces are written only together with the settings row.
    /// A later change of a list or of a setting default is a new migration; this file is never edited (09 10.6 item 7).
    /// </para>
    /// <para>
    /// <b>Not start-up data (06 8.3, 8.4) - none of these is written by a migration:</b>
    /// in the code - the product constants, the enums (the unit of measure included), the business day calendar, the common password list,
    /// the carrier list and the tracking patterns, the domestic production logo and the templates of the two generated texts;
    /// at installation - the first administrator (a management command) and the installation settings (domain, sender address, provider
    /// keys, Google application), which are not in the database;
    /// at the application's first start - the first versions of the two generated legal texts (<c>PreInformationForm</c>,
    /// <c>DistanceSalesContract</c>), written by the template version comparison as a system event outside the audit trail (T366);
    /// by the framework - the <c>DataProtectionKey</c> rows of the key ring.
    /// </para>
    /// </summary>
    public partial class InsertStartupData : Migration
    {
        /// <summary>The migration's own moment: the creation time of every row and the time part of every id.</summary>
        internal static readonly DateTimeOffset Moment = new(2026, 11, 4, 13, 47, 54, TimeSpan.Zero);

        /// <summary>The firm setting defaults of 02 11.1 that are columns of <c>company_settings</c>; an empty value is "Boş" (NULL).</summary>
        internal static readonly SettingDefault[] SettingDefaults =
        [
            new("P-1", "shipping_fee", 0.00m),
            new("P-3", "free_shipping_threshold", null),
            new("P-4", "minimum_order_amount", null),
            new("P-5", "default_shipping_period", 2m),
            new("P-7", "bank_transfer_payment_window", 2m),
            new("P-8", "download_allowance", 5m),
            new("P-10", "default_vat_rate", 20.00m),
        ];

        /// <summary>The homepage layout of a new installation (06 8.1.1; 02 3.28.1).</summary>
        internal const string HomepageLayout = "ContentFirst";

        /// <summary>The menu names that come with the product (04 9.14.4; 06 4.2).</summary>
        internal static readonly (string Column, string Name)[] MenuNames =
        [
            ("menu_name_products", "Ürünler"),
            ("menu_name_offerings", "Hizmetlerimiz"),
            ("menu_name_portfolio_items", "Referanslarımız"),
            ("menu_name_about_page", "Hakkımızda"),
            ("menu_name_faq_items", "SSS"),
            ("menu_name_contact", "İletişim"),
        ];

        /// <summary>The 81 provinces; the code is the plate (06 4.3).</summary>
        internal static readonly (string Code, string Name)[] Provinces =
        [
            ("01", "Adana"), ("02", "Adıyaman"), ("03", "Afyonkarahisar"), ("04", "Ağrı"), ("05", "Amasya"), ("06", "Ankara"),
            ("07", "Antalya"), ("08", "Artvin"), ("09", "Aydın"), ("10", "Balıkesir"), ("11", "Bilecik"), ("12", "Bingöl"),
            ("13", "Bitlis"), ("14", "Bolu"), ("15", "Burdur"), ("16", "Bursa"), ("17", "Çanakkale"), ("18", "Çankırı"),
            ("19", "Çorum"), ("20", "Denizli"), ("21", "Diyarbakır"), ("22", "Edirne"), ("23", "Elazığ"), ("24", "Erzincan"),
            ("25", "Erzurum"), ("26", "Eskişehir"), ("27", "Gaziantep"), ("28", "Giresun"), ("29", "Gümüşhane"), ("30", "Hakkari"),
            ("31", "Hatay"), ("32", "Isparta"), ("33", "Mersin"), ("34", "İstanbul"), ("35", "İzmir"), ("36", "Kars"),
            ("37", "Kastamonu"), ("38", "Kayseri"), ("39", "Kırklareli"), ("40", "Kırşehir"), ("41", "Kocaeli"), ("42", "Konya"),
            ("43", "Kütahya"), ("44", "Malatya"), ("45", "Manisa"), ("46", "Kahramanmaraş"), ("47", "Mardin"), ("48", "Muğla"),
            ("49", "Muş"), ("50", "Nevşehir"), ("51", "Niğde"), ("52", "Ordu"), ("53", "Rize"), ("54", "Sakarya"),
            ("55", "Samsun"), ("56", "Siirt"), ("57", "Sinop"), ("58", "Sivas"), ("59", "Tekirdağ"), ("60", "Tokat"),
            ("61", "Trabzon"), ("62", "Tunceli"), ("63", "Şanlıurfa"), ("64", "Uşak"), ("65", "Van"), ("66", "Yozgat"),
            ("67", "Zonguldak"), ("68", "Aksaray"), ("69", "Bayburt"), ("70", "Karaman"), ("71", "Kırıkkale"), ("72", "Batman"),
            ("73", "Şırnak"), ("74", "Bartın"), ("75", "Ardahan"), ("76", "Iğdır"), ("77", "Yalova"), ("78", "Karabük"),
            ("79", "Kilis"), ("80", "Osmaniye"), ("81", "Düzce"),
        ];

        /// <summary>The officially assigned ISO 3166-1 alpha-2 codes with their Turkish names (06 4.4).</summary>
        internal static readonly (string Code, string Name)[] Countries =
        [
            ("AD", "Andorra"), ("AE", "Birleşik Arap Emirlikleri"), ("AF", "Afganistan"), ("AG", "Antigua ve Barbuda"), ("AI", "Anguilla"),
            ("AL", "Arnavutluk"), ("AM", "Ermenistan"), ("AO", "Angola"), ("AQ", "Antarktika"), ("AR", "Arjantin"), ("AS", "Amerikan Samoası"),
            ("AT", "Avusturya"), ("AU", "Avustralya"), ("AW", "Aruba"), ("AX", "Åland Adaları"), ("AZ", "Azerbaycan"),
            ("BA", "Bosna-Hersek"), ("BB", "Barbados"), ("BD", "Bangladeş"), ("BE", "Belçika"), ("BF", "Burkina Faso"), ("BG", "Bulgaristan"),
            ("BH", "Bahreyn"), ("BI", "Burundi"), ("BJ", "Benin"), ("BL", "Saint Barthelemy"), ("BM", "Bermuda"), ("BN", "Brunei"),
            ("BO", "Bolivya"), ("BQ", "Karayip Hollandası"), ("BR", "Brezilya"), ("BS", "Bahamalar"), ("BT", "Butan"), ("BV", "Bouvet Adası"),
            ("BW", "Botsvana"), ("BY", "Belarus"), ("BZ", "Belize"),
            ("CA", "Kanada"), ("CC", "Cocos (Keeling) Adaları"), ("CD", "Kongo Demokratik Cumhuriyeti"), ("CF", "Orta Afrika Cumhuriyeti"),
            ("CG", "Kongo Cumhuriyeti"), ("CH", "İsviçre"), ("CI", "Côte d'Ivoire"), ("CK", "Cook Adaları"), ("CL", "Şili"), ("CM", "Kamerun"),
            ("CN", "Çin"), ("CO", "Kolombiya"), ("CR", "Kosta Rika"), ("CU", "Küba"), ("CV", "Cabo Verde"), ("CW", "Curaçao"),
            ("CX", "Christmas Adası"), ("CY", "Kıbrıs"), ("CZ", "Çekya"),
            ("DE", "Almanya"), ("DJ", "Cibuti"), ("DK", "Danimarka"), ("DM", "Dominika"), ("DO", "Dominik Cumhuriyeti"), ("DZ", "Cezayir"),
            ("EC", "Ekvador"), ("EE", "Estonya"), ("EG", "Mısır"), ("EH", "Batı Sahra"), ("ER", "Eritre"), ("ES", "İspanya"), ("ET", "Etiyopya"),
            ("FI", "Finlandiya"), ("FJ", "Fiji"), ("FK", "Falkland Adaları"), ("FM", "Mikronezya"), ("FO", "Faroe Adaları"), ("FR", "Fransa"),
            ("GA", "Gabon"), ("GB", "Birleşik Krallık"), ("GD", "Grenada"), ("GE", "Gürcistan"), ("GF", "Fransız Guyanası"), ("GG", "Guernsey"),
            ("GH", "Gana"), ("GI", "Cebelitarık"), ("GL", "Grönland"), ("GM", "Gambiya"), ("GN", "Gine"), ("GP", "Guadeloupe"),
            ("GQ", "Ekvator Ginesi"), ("GR", "Yunanistan"), ("GS", "Güney Georgia ve Güney Sandwich Adaları"), ("GT", "Guatemala"), ("GU", "Guam"),
            ("GW", "Gine-Bissau"), ("GY", "Guyana"),
            ("HK", "Hong Kong"), ("HM", "Heard Adası ve McDonald Adaları"), ("HN", "Honduras"), ("HR", "Hırvatistan"), ("HT", "Haiti"),
            ("HU", "Macaristan"),
            ("ID", "Endonezya"), ("IE", "İrlanda"), ("IL", "İsrail"), ("IM", "Man Adası"), ("IN", "Hindistan"), ("IO", "Britanya Hint Okyanusu Toprakları"),
            ("IQ", "Irak"), ("IR", "İran"), ("IS", "İzlanda"), ("IT", "İtalya"),
            ("JE", "Jersey"), ("JM", "Jamaika"), ("JO", "Ürdün"), ("JP", "Japonya"),
            ("KE", "Kenya"), ("KG", "Kırgızistan"), ("KH", "Kamboçya"), ("KI", "Kiribati"), ("KM", "Komorlar"), ("KN", "Saint Kitts ve Nevis"),
            ("KP", "Kuzey Kore"), ("KR", "Güney Kore"), ("KW", "Kuveyt"), ("KY", "Cayman Adaları"), ("KZ", "Kazakistan"),
            ("LA", "Laos"), ("LB", "Lübnan"), ("LC", "Saint Lucia"), ("LI", "Liechtenstein"), ("LK", "Sri Lanka"), ("LR", "Liberya"),
            ("LS", "Lesotho"), ("LT", "Litvanya"), ("LU", "Lüksemburg"), ("LV", "Letonya"), ("LY", "Libya"),
            ("MA", "Fas"), ("MC", "Monako"), ("MD", "Moldova"), ("ME", "Karadağ"), ("MF", "Saint Martin"), ("MG", "Madagaskar"),
            ("MH", "Marshall Adaları"), ("MK", "Kuzey Makedonya"), ("ML", "Mali"), ("MM", "Myanmar"), ("MN", "Moğolistan"), ("MO", "Makao"),
            ("MP", "Kuzey Mariana Adaları"), ("MQ", "Martinik"), ("MR", "Moritanya"), ("MS", "Montserrat"), ("MT", "Malta"), ("MU", "Mauritius"),
            ("MV", "Maldivler"), ("MW", "Malavi"), ("MX", "Meksika"), ("MY", "Malezya"), ("MZ", "Mozambik"),
            ("NA", "Namibya"), ("NC", "Yeni Kaledonya"), ("NE", "Nijer"), ("NF", "Norfolk Adası"), ("NG", "Nijerya"), ("NI", "Nikaragua"),
            ("NL", "Hollanda"), ("NO", "Norveç"), ("NP", "Nepal"), ("NR", "Nauru"), ("NU", "Niue"), ("NZ", "Yeni Zelanda"),
            ("OM", "Umman"),
            ("PA", "Panama"), ("PE", "Peru"), ("PF", "Fransız Polinezyası"), ("PG", "Papua Yeni Gine"), ("PH", "Filipinler"), ("PK", "Pakistan"),
            ("PL", "Polonya"), ("PM", "Saint Pierre ve Miquelon"), ("PN", "Pitcairn Adaları"), ("PR", "Porto Riko"), ("PS", "Filistin"),
            ("PT", "Portekiz"), ("PW", "Palau"), ("PY", "Paraguay"),
            ("QA", "Katar"),
            ("RE", "Reunion"), ("RO", "Romanya"), ("RS", "Sırbistan"), ("RU", "Rusya"), ("RW", "Ruanda"),
            ("SA", "Suudi Arabistan"), ("SB", "Solomon Adaları"), ("SC", "Seyşeller"), ("SD", "Sudan"), ("SE", "İsveç"), ("SG", "Singapur"),
            ("SH", "Saint Helena"), ("SI", "Slovenya"), ("SJ", "Svalbard ve Jan Mayen"), ("SK", "Slovakya"), ("SL", "Sierra Leone"),
            ("SM", "San Marino"), ("SN", "Senegal"), ("SO", "Somali"), ("SR", "Surinam"), ("SS", "Güney Sudan"), ("ST", "São Tomé ve Príncipe"),
            ("SV", "El Salvador"), ("SX", "Sint Maarten"), ("SY", "Suriye"), ("SZ", "Esvatini"),
            ("TC", "Turks ve Caicos Adaları"), ("TD", "Çad"), ("TF", "Fransız Güney Toprakları"), ("TG", "Togo"), ("TH", "Tayland"),
            ("TJ", "Tacikistan"), ("TK", "Tokelau"), ("TL", "Timor-Leste"), ("TM", "Türkmenistan"), ("TN", "Tunus"), ("TO", "Tonga"),
            ("TR", "Türkiye"), ("TT", "Trinidad ve Tobago"), ("TV", "Tuvalu"), ("TW", "Tayvan"), ("TZ", "Tanzanya"),
            ("UA", "Ukrayna"), ("UG", "Uganda"), ("UM", "ABD Küçük Harici Adaları"), ("US", "Amerika Birleşik Devletleri"), ("UY", "Uruguay"),
            ("UZ", "Özbekistan"),
            ("VA", "Vatikan"), ("VC", "Saint Vincent ve Grenadinler"), ("VE", "Venezuela"), ("VG", "Britanya Virjin Adaları"),
            ("VI", "ABD Virjin Adaları"), ("VN", "Vietnam"), ("VU", "Vanuatu"),
            ("WF", "Wallis ve Futuna"), ("WS", "Samoa"),
            ("YE", "Yemen"), ("YT", "Mayotte"),
            ("ZA", "Güney Afrika"), ("ZM", "Zambiya"), ("ZW", "Zimbabve"),
        ];

        /// <summary>
        /// The privacy notice draft (02 3.33.2, 12.2): the company's identity fields are tokens of the field names of 06 4.1, the parts the
        /// company writes itself are in square brackets. The draft uses only the closed text format set (09 15).
        /// </summary>
        internal const string PrivacyNoticeDraft = """
            <h3>Veri sorumlusu</h3>
            <p>Kişisel verileriniz, 6698 sayılı Kişisel Verilerin Korunması Kanunu ("KVKK") uyarınca veri sorumlusu sıfatıyla <strong>{{LegalName}}</strong> tarafından işlenir. Adres: {{AddressLine}}, {{District}} / {{ProvinceId}} · E-posta: {{Email}} · KEP: {{KepAddress}} · Telefon: {{Phone}}</p>
            <h3>İşlenen kişisel veriler</h3>
            <ul>
            <li>Kimlik: ad ve soyad</li>
            <li>İletişim: e-posta adresi, telefon numarası, teslimat ve fatura adresi</li>
            <li>Müşteri işlemi: sipariş, sepet, iade, cayma ve talep kayıtları</li>
            <li>Finans: geri ödeme için bildirdiğiniz IBAN</li>
            <li>İşlem güvenliği: oturum, giriş ve bağlantı kayıtları</li>
            </ul>
            <h3>İşleme amaçları ve hukuki sebepleri</h3>
            <ul>
            <li>Siparişin kurulması, ödemesinin alınması, teslimi, iadesi ve geri ödemesi: bir sözleşmenin kurulması ve ifası (KVKK m.5/2-c).</li>
            <li>Fatura ve muhasebe kayıtlarının tutulması, ticari kayıtların saklanması: hukuki yükümlülüğün yerine getirilmesi (KVKK m.5/2-ç); elektronik ticaret kayıtları on yıl saklanır (6563 sayılı Kanun m.11/3).</li>
            <li>Hesabınızın ve sitenin güvenliği: meşru menfaat (KVKK m.5/2-f).</li>
            <li>Taleplerinizin ve uyuşmazlıkların yönetimi: bir hakkın tesisi, kullanılması veya korunması (KVKK m.5/2-e).</li>
            <li>[İşleme amaçlarını ve hukuki sebeplerini firmanızın işine göre gözden geçirin]</li>
            </ul>
            <h3>Aktarılan alıcı grupları</h3>
            <ul>
            <li>Kargo ve teslimat şirketleri: [şirketlerin adları]</li>
            <li>Fatura ve muhasebe hizmeti sağlayıcıları: [sağlayıcıların adları]</li>
            <li>Ödeme kuruluşu: [ödeme sağlayıcısının adı] - ödemenin başlatılması ve geri ödeme için kimlik ve iletişim verileriniz aktarılır.</li>
            <li>Barındırma ve e-posta altyapısı (veri işleyen): [sağlayıcıların adları]</li>
            <li>Bakım ve destek sağlayıcısı (veri işleyen): [sağlayıcının adı]</li>
            <li>Google ile giriş: [Google ile giriş kullanılıyorsa aktarılan veriler]</li>
            <li>Yetkili kamu kurum ve kuruluşları: kanunun izin verdiği ölçüde.</li>
            </ul>
            <h3>Barındırma ve e-posta altyapısının konumu</h3>
            <p>[Sitenin barındırıldığı ve e-postaların gönderildiği altyapının konumu - ülke ve sağlayıcı]</p>
            <h3>Toplama yöntemi</h3>
            <p>Kişisel verileriniz sitedeki formlar, hesabınız ve siparişleriniz aracılığıyla elektronik ortamda toplanır.</p>
            <h3>Haklarınız</h3>
            <p>KVKK m.11 uyarınca kişisel verilerinizin işlenip işlenmediğini öğrenme, işlenmişse bilgi talep etme, işleme amacını ve amacına uygun kullanılıp kullanılmadığını öğrenme, aktarıldığı üçüncü kişileri bilme, eksik veya yanlış işlenmişse düzeltilmesini, şartları oluşmuşsa silinmesini veya yok edilmesini ve bu işlemlerin aktarılan kişilere bildirilmesini isteme, münhasıran otomatik sistemlerle analiz sonucu aleyhinize bir sonuca itiraz etme ve kanuna aykırı işleme nedeniyle uğradığınız zararın giderilmesini talep etme haklarına sahipsiniz.</p>
            <h3>Başvuru yolları</h3>
            <p>Veri Sorumlusuna Başvuru Usul ve Esasları Hakkında Tebliğ uyarınca başvurunuzu yazılı olarak {{AddressLine}}, {{District}} / {{ProvinceId}} adresine, KEP ile {{KepAddress}} adresine, güvenli elektronik imza veya mobil imza ile ya da bize daha önce bildirdiğiniz ve sistemimizde kayıtlı e-posta adresinizden {{Email}} adresine iletebilirsiniz. Başvurunuz en geç otuz gün içinde ücretsiz sonuçlandırılır. Başvurunuz reddedilirse, cevabı yetersiz bulursanız ya da süresinde cevap verilmezse Kişisel Verileri Koruma Kurulu'na şikâyette bulunabilirsiniz (KVKK m.14).</p>
            """;

        /// <summary>The cookie policy draft (02 12.2.5): only strictly necessary first-party cookies; the payment provider's cookies are the company's part.</summary>
        internal const string CookiePolicyDraft = """
            <h3>Çerez nedir</h3>
            <p>Çerez, ziyaret ettiğiniz sitenin tarayıcınıza yazdığı küçük bir metin dosyasıdır.</p>
            <h3>Kullandığımız çerezler</h3>
            <p>Bu sitede yalnız sitenin çalışması için zorunlu çerezler vardır ve ödeme sağlayıcısının çerçevesi dışında hepsi <strong>{{LegalName}}</strong> tarafından yazılır:</p>
            <ul>
            <li>Oturum: girişinizi açık tutar.</li>
            <li>Beni hatırla: istediğinizde girişinizi tarayıcıda hatırlar.</li>
            <li>Misafir sepeti: giriş yapmadan oluşturduğunuz sepeti tarayıcınıza bağlar; kalıcıdır ve dayanağı satış sözleşmesinin kurulmasıdır (KVKK m.5/2-c).</li>
            <li>Form güvenliği: formların sizin tarafınızdan gönderildiğini doğrular.</li>
            <li>Google ile giriş akışı: girişin dönüşünü eşleyen tek kullanımlık değerleri taşır; ömrü giriş akışı kadardır, dakikalar.</li>
            <li>Tanınan tarayıcı işareti: girişte tarayıcınızı tanıyan güvenlik çerezidir.</li>
            <li>Ödeme adımı: ödeme adımında yazdıklarınızı o tarayıcı oturumu boyunca korur ve sipariş onaylanınca silinir.</li>
            </ul>
            <p>Bu çerezler zorunlu olduğu için açık rızanızı gerektirmez. Üçüncü taraf çerezi, izleme pikseli ve reklam etiketi kullanılmaz.</p>
            <h3>Ödeme sağlayıcısının çerezleri</h3>
            <p>[Ödeme sağlayıcınızın çerçevesi kullanılıyorsa sağlayıcının ödeme adımında yazdığı zorunlu çerezler]</p>
            <h3>Çerezleri yönetme</h3>
            <p>Çerezleri tarayıcınızın ayarlarından silebilir ya da engelleyebilirsiniz; zorunlu çerezler engellenirse site düzgün çalışmayabilir.</p>
            <h3>İletişim</h3>
            <p>Sorularınız için {{Email}} adresine yazabilirsiniz.</p>
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ListSql("provinces", 1, Provinces));
            migrationBuilder.Sql(ListSql("countries", 2, Countries));
            migrationBuilder.Sql(SingleRowSql("companies", 4));
            migrationBuilder.Sql(SettingsAndDeliveryProvincesSql());
            migrationBuilder.Sql(SingleRowSql("brandings", 6));
            migrationBuilder.Sql(SingleRowSql("channel_state_records", 7));
            migrationBuilder.Sql(SingleRowSql("heartbeat_records", 8));
            migrationBuilder.Sql(SingleRowSql("backup_status_records", 9));
            migrationBuilder.Sql(DraftSql("PrivacyNotice", 1, PrivacyNoticeDraft));
            migrationBuilder.Sql(DraftSql("CookiePolicy", 2, CookiePolicyDraft));
        }

        /// <summary>A list row is matched by its code: a code that is already there is left as it is.</summary>
        private static string ListSql(string table, int tableNumber, (string Code, string Name)[] rows)
        {
            string values = string.Join(
                ",\n",
                rows.Select((row, i) => $"({Literal(Id(tableNumber, i + 1))}, {MomentLiteral}, {Literal(row.Code)}, {Literal(row.Name)})"));
            return $"INSERT INTO public.{table} (id, created_at, code, name) VALUES\n{values}\nON CONFLICT (code) DO NOTHING;";
        }

        /// <summary>A single row takes every other column from the database's default; its single-row index keeps a second one out.</summary>
        private static string SingleRowSql(string table, int tableNumber)
        {
            return $"INSERT INTO public.{table} (id, created_at) VALUES ({Literal(Id(tableNumber, 1))}, {MomentLiteral}) ON CONFLICT DO NOTHING;";
        }

        /// <summary>
        /// The settings row with the 02 11.1 defaults, the layout and the menu names (the switches and the section versions are the database's
        /// defaults), and with it the 81 delivery provinces: they are written only when the settings row is, so a province the company
        /// deselected does not come back.
        /// </summary>
        private static string SettingsAndDeliveryProvincesSql()
        {
            string columns = string.Join(", ", SettingDefaults.Select(s => s.Column).Concat(["homepage_layout"]).Concat(MenuNames.Select(m => m.Column)));
            string values = string.Join(
                ", ",
                SettingDefaults.Select(s => s.Value is decimal value ? value.ToString(CultureInfo.InvariantCulture) : "NULL")
                    .Concat([Literal(HomepageLayout)])
                    .Concat(MenuNames.Select(m => Literal(m.Name))));
            string deliveries = string.Join(", ", Provinces.Select((p, i) => $"({Literal(Id(3, i + 1))}, {Literal(p.Code)})"));

            StringBuilder sql = new();
            sql.Append("WITH settings AS (\n");
            sql.Append($"  INSERT INTO public.company_settings (id, created_at, {columns})\n");
            sql.Append($"  VALUES ({Literal(Id(5, 1))}, {MomentLiteral}, {values})\n");
            sql.Append("  ON CONFLICT DO NOTHING\n");
            sql.Append("  RETURNING id)\n");
            sql.Append("INSERT INTO public.company_delivery_provinces (id, created_at, province_id)\n");
            sql.Append($"SELECT v.id::uuid, {MomentLiteral}::timestamptz, p.id\n");
            sql.Append($"FROM (VALUES {deliveries}) AS v (id, code)\n");
            sql.Append("JOIN public.provinces AS p ON p.code = v.code\n");
            sql.Append("WHERE EXISTS (SELECT 1 FROM settings);");
            return sql.ToString();
        }

        /// <summary>A draft is written only when its type has no row yet; the publish time and the version number stay empty.</summary>
        private static string DraftSql(string type, int number, string text)
        {
            return $"INSERT INTO public.legal_text_versions (id, created_at, legal_text_type, text)\n"
                + $"SELECT {Literal(Id(10, number))}::uuid, {MomentLiteral}::timestamptz, {Literal(type)}, {Literal(text.ReplaceLineEndings("\n"))}\n"
                + $"WHERE NOT EXISTS (SELECT 1 FROM public.legal_text_versions WHERE legal_text_type = {Literal(type)});";
        }

        private static string MomentLiteral => Literal(Moment.ToString("yyyy-MM-dd HH:mm:ss+00", CultureInfo.InvariantCulture));

        /// <summary>
        /// A fixed version 7 UUID: the migration's moment in the 48-bit time field, the table's number in the 12 bits after the version and the
        /// row's number in the last 48 bits (RFC 9562 allows a counter in place of the random bits).
        /// </summary>
        internal static string Id(int tableNumber, int rowNumber)
        {
            long ms = Moment.ToUnixTimeMilliseconds();
            return string.Create(CultureInfo.InvariantCulture, $"{ms >> 16:x8}-{ms & 0xFFFF:x4}-7{tableNumber:x3}-8000-{rowNumber:x12}");
        }

        private static string Literal(string value) => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

        /// <summary>One firm setting default: its parameter number in 02 11.1, its column and its value (empty is NULL).</summary>
        internal sealed record SettingDefault(string Parameter, string Column, decimal? Value);
    }
}
