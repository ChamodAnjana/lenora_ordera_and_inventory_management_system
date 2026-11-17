namespace lenora_ordera_and_inventory_management_system.Common;

internal static class Rules
{
    public const int BcryptWorkFactor = 12;

    public const int FullNameMaxLength = 100;

    public const int UsernameMaxLength = 50;

    public const int PasswordMinLength = 8;

    public const int PasswordHashMaxLength = 255;

    public const int RoleMaxLength = 20;

    public const int ProductNameMaxLength = 100;

    public const int ProductCategoryMaxLength = 50;

    public const int WholesalerNameMaxLength = 100;

    public const int WholesalerContactNoMaxLength = 20;

    public const int WholesalerAddressMaxLength = 200;

    public const int CustomerNameMaxLength = 100;

    public const int CustomerPhoneMaxLength = 20;

    public const int CustomerSearchLimit = 50;

    public const int SriLankanMobileLength = 10;

    public const int SriLankanMobileWithCountryLength = 11;

    public const string SriLankanTrunkPrefix = "0";

    public const string SriLankanMobilePrefix = "07";

    public const string SriLankanCountryCode = "94";

    public const int CustomerAddressMaxLength = 200;

    public const int CourierNameMaxLength = 50;

    public const int FollowUpDays = 2;
}
