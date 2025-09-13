using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

return await Deployment.RunAsync(() =>
{
    // Configuration
    var config = new Config();
    var environment = config.Get("environment") ?? "dev";
    var projectName = config.Get("projectName") ?? "overblikplus";
    
    // Resource group
    var resourceGroup = new ResourceGroup($"{projectName}-{environment}-rg", new ResourceGroupArgs
    {
        Location = "West Europe"
    });

    // Storage account for blob storage
    var storageAccount = new StorageAccount($"{projectName}{environment}storage", new StorageAccountArgs
    {
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        Sku = new SkuArgs
        {
            Name = SkuName.Standard_LRS
        },
        Kind = Kind.StorageV2,
        AccessTier = AccessTier.Hot,
        AllowBlobPublicAccess = true
    });

    // Storage container for blob storage
    var blobContainer = new BlobContainer("images", new BlobContainerArgs
    {
        AccountName = storageAccount.Name,
        ResourceGroupName = resourceGroup.Name,
        PublicAccess = PublicAccess.Blob
    });

    // SQL Server
    var sqlServer = new Server($"{projectName}-{environment}-sql", new ServerArgs
    {
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        AdministratorLogin = "overblikplusadmin",
        AdministratorLoginPassword = Output.CreateSecret(config.GetSecret("sqlAdminPassword") ?? "OverblikPlus2024!Strong"),
        Version = "12.0"
    });

    // SQL Database
    var sqlDatabase = new Database($"{projectName}-{environment}-db", new DatabaseArgs
    {
        ResourceGroupName = resourceGroup.Name,
        ServerName = sqlServer.Name,
        Location = resourceGroup.Location,
        Sku = new SkuArgs
        {
            Name = "S0", // Basic tier for development, change to S1, S2, etc. for production
            Tier = "Standard"
        },
        Collation = "SQL_Latin1_General_CP1_CI_AS",
        MaxSizeBytes = 268435456000, // 250 GB
        RequestedServiceObjectiveName = "S0"
    });

    // Firewall rule to allow Azure services
    var firewallRule = new FirewallRule("AllowAzureServices", new FirewallRuleArgs
    {
        ResourceGroupName = resourceGroup.Name,
        ServerName = sqlServer.Name,
        StartIpAddress = "0.0.0.0",
        EndIpAddress = "0.0.0.0"
    });

    // Firewall rule for your IP (you'll need to update this)
    var clientFirewallRule = new FirewallRule("AllowClientIP", new FirewallRuleArgs
    {
        ResourceGroupName = resourceGroup.Name,
        ServerName = sqlServer.Name,
        StartIpAddress = "0.0.0.0", // Replace with your actual IP
        EndIpAddress = "255.255.255.255" // Replace with your actual IP
    });

    // Azure Static Web App
    var staticWebApp = new StaticSite($"{projectName}-{environment}-swa", new StaticSiteArgs
    {
        ResourceGroupName = resourceGroup.Name,
        Location = "WestEurope2",
        Sku = new SkuDescriptionArgs
        {
            Name = "Free",
            Tier = "Free"
        },
        BuildProperties = new StaticSiteBuildPropertiesArgs
        {
            AppLocation = "/",
            ApiLocation = "",
            OutputLocation = "wwwroot"
        }
    });

    // Outputs
    return new Dictionary<string, object?>
    {
        { "resourceGroupName", resourceGroup.Name },
        { "sqlServerName", sqlServer.Name },
        { "sqlDatabaseName", sqlDatabase.Name },
        { "sqlServerFqdn", sqlServer.FullyQualifiedDomainName },
        { "storageAccountName", storageAccount.Name },
        { "storageAccountKey", storageAccount.PrimaryAccessKey },
        { "blobContainerName", blobContainer.Name },
        { "connectionString", Output.Format($"Server=tcp:{sqlServer.FullyQualifiedDomainName},1433;Initial Catalog={sqlDatabase.Name};Persist Security Info=False;User ID=overblikplusadmin;Password={config.GetSecret("sqlAdminPassword") ?? "OverblikPlus2024!Strong"};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;") },
        { "blobConnectionString", Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={storageAccount.PrimaryAccessKey};EndpointSuffix=core.windows.net") },
        { "blobBaseUrl", Output.Format($"https://{storageAccount.Name}.blob.core.windows.net/{blobContainer.Name}") },
        { "staticWebAppUrl", staticWebApp.DefaultHostName },
        { "staticWebAppName", staticWebApp.Name }
    };
});