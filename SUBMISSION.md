# Submission email template

Copy and send as a reply to the assignment email.

---

**Subject:** Re: Technical Challenge Submission — WeatherAI API Integration

Hi,

Please find my submission below:

**GitHub repository (public):**  
https://github.com/mburu1/weatherai-integration

**Live deployment:**  
https://weatherai-integration.onrender.com

**Swagger UI (interactive testing):**  
https://weatherai-integration.onrender.com/swagger

**Quick test endpoint (Nairobi weather):**  
https://weatherai-integration.onrender.com/api/weather?lat=-1.2921&lon=36.8219&days=5&ai=false&units=metric&lang=en

**Dashboard summary (curated view):**  
https://weatherai-integration.onrender.com/api/summary?lat=-1.2921&lon=36.8219&days=5&ai=false&units=metric

**Compare locations (Nairobi vs New York):**  
https://weatherai-integration.onrender.com/api/compare?locations=-1.2921,36.8219|40.7128,-74.0060&days=3&units=metric

### Project summary

- **Stack:** .NET 10, ASP.NET Core Minimal APIs, Swashbuckle Swagger UI, xUnit
- **Integration:** WeatherAI `GET /v1/weather` (and related endpoints) via a typed `HttpClient` library
- **Design:** Multi-project solution (Contracts, Client, Api, Tests) with server-side API key handling
- **Beyond a simple proxy:** Application service layer with in-memory caching, Polly HTTP resilience, curated summary/compare endpoints, health probes, and structured error handling
- **Docs:** Full setup instructions in `README.md`

Thank you for the opportunity. I look forward to hearing from you.

Best regards,  
Mwangi Wa Mburu

---