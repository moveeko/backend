namespace backend.Companies;

public class Company
{
    public string CompanyId;
    public string CompanyName;
    public string? CompanyEmail;

    public List<int> workers;

    public Company(string id, string? companyEmail)
    {
        CompanyId = id;
        CompanyEmail = companyEmail;
    }

    public Company(string id, string? companyEmail, string companyName, List<int> ids)
    {
        CompanyId = id;
        CompanyEmail = companyEmail;
        CompanyName = companyName;
        workers = ids;
    }


    public async Task AddWorkers()
    {
        
    }
}