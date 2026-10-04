# Live Coding Challenges

Desafios de estudo para entrevistas com live coding (.NET + React).

## Challenge 1 — Partner Integration API (.NET) ⏱ 60 min

**Scenario:** A construction software company needs to show heavy equipment available for rent from multiple suppliers. Each supplier has its own API format. Build an ASP.NET Core (.NET 8) API that aggregates and normalizes this data.

Supplier A (EquipRent) returns:

```json
[
  {
    "equipment_id": "EX-001",
    "desc": "Excavator CAT 320",
    "daily_rate_usd": 850.0,
    "available": true,
    "last_update": "2026-09-28T14:00:00Z"
  }
]
```

Supplier B (HeavyMach) returns:

```json
{
  "items": [
    {
      "code": 4471,
      "name": "Bulldozer D6",
      "pricing": { "amountInCents": 420000, "currency": "USD", "unit": "week" },
      "status": "AVAILABLE"
    }
  ]
}
```

`status` can be `AVAILABLE`, `RENTED` or `MAINTENANCE`. `unit` can be `day` or `week`.

**Requirements:**

- Create a unified model: `Id`, `Name`, `DailyRate` (USD), `IsAvailable`, `Supplier`.
- Endpoint `GET /api/equipment?available=true&maxDailyRate=1000`, sorted by `DailyRate` ascending.
- Suppliers can be faked (in-memory class or local JSON file), but each must sit behind an interface.
- If one supplier fails, still return data from the others and include a warning in the response.

**Extras (se sobrar tempo):** chamar os fornecedores em paralelo com `Task.WhenAll`, timeout por fornecedor, testes unitários com xUnit para o mapeamento, cache de 1 minuto.

**O que é avaliado:** separação de responsabilidades (Adapter pattern), async correto, tratamento de falha parcial e conversão de unidades (semana → dia, centavos → dólar).

## Challenge 2 — Equipment Dashboard (React 18 + TS) ⏱ 45 min

**Scenario:** Build a frontend that consumes the API from Challenge 1. If you haven't done it yet, use a mock JSON.

**Requirements:**

- List equipment showing name, supplier, daily rate (formatted as currency) and availability.
- Search by name (case-insensitive).
- Toggle "only available".
- Sort by price (asc/desc).
- Handle loading, error and empty states.
- Show a warning banner if the API returns supplier warnings.

**Extras:** extrair um custom hook `useEquipment`, debounce na busca, `AbortController` no fetch, componentização limpa.

## Challenge 3 — Bug Hunt ⏱ 30 min

Encontre e corrija os bugs. Explique cada um em voz alta, em inglês, como se estivesse na entrevista. Cada snippet tem pelo menos 5 problemas.

### C#

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddSingleton<ITimesheetService, TimesheetService>(); // TimesheetService uses AppDbContext

[ApiController]
[Route("api/[controller]")]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _service;
    public TimesheetsController(ITimesheetService service) => _service = service;

    [HttpGet("{employeeId}/total-hours")]
    public IActionResult GetTotalHours(int employeeId, DateTime start, DateTime end)
    {
        var entries = _service.GetEntriesAsync(employeeId).Result;
        double total = 0;
        for (int i = 0; i <= entries.Count; i++)
        {
            if (entries[i].Date > start && entries[i].Date < end)
                total += entries[i].Hours;
        }
        return Ok(total);
    }

    [HttpPost]
    public async void Create(TimesheetEntry entry)
    {
        if (entry.Hours > 24 || entry.Hours < 0) BadRequest();
        await _service.SaveAsync(entry);
    }
}
```

### React

```tsx
function ProjectList({ status }: { status: string }) {
  const [projects, setProjects] = useState<Project[]>([]);
  const [search, setSearch] = useState("");

  useEffect(() => {
    fetch(`/api/projects?status=${status}`)
      .then(r => r.json())
      .then(data => setProjects(data));
  });

  const handleArchive = (id: number) => {
    const p = projects.find(p => p.id === id)!;
    p.archived = true;
    setProjects(projects);
  };

  const filtered = projects.filter(p => p.name.includes(search));

  return (
    <div>
      <input value={search} />
      {filtered.map((p, index) => (
        <div key={index}>
          {p.name}
          <button onClick={handleArchive(p.id)}>Archive</button>
        </div>
      ))}
    </div>
  );
}
```

## Challenge 4 — Refactoring ⏱ 30 min

Refatore o código abaixo tornando-o testável e seguro, e aplique SOLID. Antes de codar, liste em voz alta tudo que está errado.

```csharp
public class EquipmentService
{
    public string ProcessMaintenance(int equipmentId, string type)
    {
        var conn = new SqlConnection("Server=prod;Database=equipment;User=sa;Password=123");
        conn.Open();
        var cmd = new SqlCommand("SELECT * FROM Equipment WHERE Id = " + equipmentId, conn);
        var reader = cmd.ExecuteReader();
        reader.Read();
        var hours = (int)reader["EngineHours"];

        double cost = 0;
        if (type == "excavator") cost = hours * 1.5 + 200;
        else if (type == "bulldozer") cost = hours * 2.0 + 350;
        else if (type == "crane") cost = hours * 3.2 + 500;

        var smtp = new SmtpClient("smtp.company.com");
        smtp.Send("noreply@company.com", "maintenance@company.com",
                  "Maintenance", $"Equipment {equipmentId} cost: {cost}");
        return "OK";
    }
}
```

> **Dica:** pense em connection string hardcoded, SQL injection, recursos sem `using`, código síncrono, Open/Closed no cálculo de custo (Strategy), dependências concretas (SMTP e banco) e um retorno sem significado.

## Challenge 5 — Algorithm Warm-ups (C#) ⏱ 15–20 min cada

1. **Weekly overtime:** given a list of `TimeEntry(EmployeeId, Date, Hours)`, return the employees who worked more than 40 hours in any single week. *(Treina LINQ com `GroupBy`.)*
2. **Merge reservations:** given equipment reservations as date intervals `[start, end]`, merge the overlapping ones. Input `[[1,3],[2,6],[8,10],[9,12]]` → `[[1,6],[8,12]]`. *(Clássico Merge Intervals, nível medium.)*
3. **Budget match:** given a list of material costs and a budget, return the indexes of two materials whose costs sum exactly to the budget, in O(n). *(Two Sum com `Dictionary`.)*

Para cada um, diga a complexidade de tempo e espaço da sua solução.

## Challenge 6 — System Design Talk (só falar, sem codar) ⏱ 10 min

Explique em inglês, em voz alta, como você resolveria isto:

> A supplier sends a webhook when a material delivery is confirmed at a construction site. The system must: (1) update the project inventory, (2) notify the site manager, and (3) generate an invoice. The supplier sometimes sends the same webhook twice, and the invoice service is occasionally down.

Tente encaixar na resposta: Azure Function (HTTP trigger), Service Bus, idempotência (chave por ID do evento), retry e dead-letter queue, e Durable Functions para orquestrar os três passos.

## Ordem sugerida

1. Comece pelo **3 (bug hunt)**, que é rápido e mostra suas lacunas.
2. Depois faça o **1** e o **2**, que são o coração dos desafios.
3. Use o **5** como aquecimento diário.
4. Deixe o **4** e o **6** para os últimos dias.

Grave sua tela uma vez para ver como você soa explicando em inglês. Ajuda muito.
