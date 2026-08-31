# QA Test Task — Limestone Digital

A small C# / .NET 8 / xUnit test framework with three UI checks against SauceDemo and five API checks
against JSONPlaceholder, plus a Postman collection and the SQL query.

Written to be read rather than to be big. The reasoning is in **Design decisions** and
**Framework structure** below; that is where the real answer to this task lives.

---

## Contents

| Path | What it is |
|---|---|
| `src/Limestone.Tests` | The framework and the tests |
| `postman/` | Exported collection and environment |
| `sql/united-package-customers.sql` | Part 3 answer |
| `Dockerfile` | Runs the suite in a container with a real Chrome |
| `.github/workflows/tests.yml` | CI pipeline |

---

## Install and run

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) and Google Chrome.
Driver binaries are resolved automatically by Selenium Manager, so there is nothing else to install.

```bash
dotnet restore
dotnet build

# the whole suite
dotnet test

# API only / UI only / smoke only  (xUnit traits)
dotnet test --filter "Category=API"
dotnet test --filter "Category=UI"
dotnet test --filter "Category=Smoke"

# a single test, by name
dotnet test --filter "FullyQualifiedName~CartTests.StandardUser_CanAddItemToCart"

# watch a UI test run in a real window
UI__HEADLESS=false dotnet test --filter "Category=UI"     # PowerShell: $env:UI__HEADLESS="false"
```

In a container:

```bash
docker build -t limestone-qa .
docker run --rm -v "$PWD/TestResults:/app/TestResults" limestone-qa
```

## Where the results land

- Console output, including the request/response log for every API call, prints as the run goes.
- A `.trx` file for CI and IDE consumption:
  `dotnet test --logger "trx;LogFileName=results.trx" --results-directory TestResults`
- **Screenshots on UI failure** are written to `TestResults/screenshots/`. xUnit v2 has no
  test-attachment API, so the file path and the failing URL are written to the test's output
  instead, which puts them in the `.trx` next to the assertion message.
  Override the location with `TEST_ARTIFACTS_DIR`.
- In CI all of it is uploaded as build artifacts (see `.github/workflows/tests.yml`).

There is no HTML report in the submission — see *Deliberately skipped*.

---

## Design decisions

- **Page Object with intent-revealing methods, not element wrappers.** Pages expose
  `LogInAs`, `AddToCart`, `OpenCart`; they return the next page object and never assert.
  Locators are private statics on the page that owns them, so no locator appears in a test.
- **A thin service layer over RestSharp instead of raw requests in tests.** `ApiClientBase` owns
  transport (base address, timeout, logging, deserialisation); `UsersClient` / `PostsClient` own
  endpoints; tests own assertions only. That separation is what makes an endpoint change a
  one-file change.
- **Configuration is layered and overridable** — `appsettings.json` → `appsettings.{TEST_ENV}.json`
  → environment variables — bound into a typed `TestSettings`. No test reads a raw key, so
  a renamed setting breaks the build rather than the run.
- **Contract assertions are made against the raw JSON, not only the deserialised model.**
  A missing field deserialises silently into `null`, so a model-only check would pass on a
  broken contract. `GetUsers_ResponseIsAnArrayOfObjectsWithRequiredKeys` walks the payload.
- **Failure evidence is captured by the framework, not by each test.** Screenshot, URL and the
  full API request/response go into the report automatically, so a red build is triageable
  without a re-run. Because xUnit's `Dispose` is not told whether the test passed, UI test bodies
  run through `Execute()` in the base class, which catches, captures and rethrows — see
  *Driver lifecycle* below for why that beats screenshotting unconditionally.
- **The framework core does not depend on the test runner.** xUnit writes per-test output through
  `ITestOutputHelper`, which is only reachable from a test class constructor, so the API clients
  would otherwise have to know what runner they are under. One small `ITestLog` interface with an
  `XunitTestLog` adapter keeps `Core/` runner-agnostic; `XunitTestLog.cs` is the only file in the
  framework that references the runner at all.
- **Deliberately skipped:** SpecFlow (adds a layer this size of suite cannot justify — worth it
  only when non-engineers actually read the features), an assertion library (FluentAssertions v8
  moved to a commercial licence; `Verify.All` covers the one thing xUnit's `Assert` genuinely
  lacks, which is reporting several failures at once), an HTML report, a schema validator library,
  and any retry mechanism.
- **With more time**, in this order: response-schema validation against a stored JSON Schema;
  a proper reporter (Allure or ReportPortal) with history and flakiness rates; a run against
  Selenium Grid in the pipeline; and the WireMock stub described below.

## Documented assumptions

1. SauceDemo credentials are public demo data, so they sit in `appsettings.json`. Real credentials
   would come from environment variables only — the shape for that is already in place
   (`CREDENTIALS__PASSWORD`), and `Credentials` is a separate section so it can be swapped whole.
2. "Assert the response contract" is read as structure and type, not exact values —
   JSONPlaceholder's data is stable but nothing guarantees it, and pinning values would make
   the tests brittle for no gain.
3. The UI journey is asserted at the cart-line level (name, quantity, price format), not by
   comparing a screenshot.
4. `GET /users/999` is expected to be a 404 rather than a 200 with an empty body. That is
   current behaviour, and it is the behaviour worth locking down.
5. The suite is expected to run against public internet endpoints; nothing is stubbed.

---

## Framework structure

### What exists here vs what is described

| Piece | Status |
|---|---|
| Layered project (core / pages / clients / tests / test data) | **Built** |
| Driver factory with local, headless and remote-grid support | **Built** — grid path is code-complete, not exercised against a real hub |
| Layered configuration with environment-variable overrides | **Built** |
| Parallel execution across test classes | **Built** — xUnit default, capped in `xunit.runner.json` |
| Screenshot + URL + API log on failure | **Built** — path logged, not attached (xUnit v2 has no attachment API) |
| Test data builder, traits, Dockerfile, CI workflow | **Built** |
| Runner-agnostic logging seam (`ITestLog`) | **Built** |
| Multi-failure assertion helper (`Verify`) | **Built** |
| SpecFlow feature file, HTML/Allure report, WireMock stub, schema validation | **Described only** |
| Multi-project split, test-data API seeding, flakiness tracking | **Described only** |

### The layers, and what each one owns

```
Core/          driver lifecycle, configuration, API transport, logging
  Config/      TestSettings (typed), TestConfig (sources and precedence)
  Drivers/     DriverFactory — the only place that knows how a browser is built
  Api/         ApiClientBase — base address, timeout, logging, deserialisation
  Logging/     ITestLog + XunitTestLog — the only file that knows the runner
  Assertions/  Verify — report every failure in a check, not just the first
Ui/
  Pages/       page objects: locators + intent methods, no assertions
  Tests/       UiTestBase (lifecycle) + the tests: assertions only
Api/
  Models/      response contracts as records
  Clients/     one client per resource, one method per operation, no assertions
  Tests/       assertions only
TestData/      named constants and builders
```

What is not allowed to leak:

- **No locator outside a page object.** If a test contains a `By`, the page is missing a method.
- **No assertion inside a page object or an API client.** They report state; the test decides
  whether that state is acceptable. This is what lets one page object serve a happy path,
  a negative path and a setup step.
- **No `IWebDriver` construction inside a test.** Only `DriverFactory` builds a driver, and only
  `UiTestBase` calls it.
- **No hard-coded URL, credential or environment name anywhere but configuration.**
- **No HTTP detail in a test.** A test asks `PostsClient` for a user's posts; whether that is a
  query parameter or a path segment is the client's business.

On a suite that has to live for years I would split these into separate projects rather than
folders — `Framework.Core`, `Product.PageObjects`, `Product.ApiClients`, `Tests.Ui`,
`Tests.Api` — because a project reference is a boundary the compiler enforces, and a folder
is only a convention people erode under deadline pressure.

### Driver lifecycle and browser configuration

xUnit constructs a new instance of the test class for **every** test and disposes it afterwards,
so the constructor is the setup and `Dispose` is the teardown, and the driver field is per-test by
construction. There is no shared instance state to guard against — which is the main reason the
parallelism story below is simpler than it would be under NUnit. `DriverFactory` is the single
construction point; nothing else builds a driver.

The one place xUnit is weaker is failure evidence. `Dispose` is not told whether the test passed,
and v2 has no attachment API, so there are three options: screenshot every test (a second wasted
per green test, and the useful images buried among hundreds of useless ones), reach into the
runner's internals by reflection (works, breaks on upgrade), or wrap the test body. I wrapped it:
`Execute()` in `UiTestBase` catches, captures the screenshot and URL, and rethrows, with the test
name supplied by `[CallerMemberName]` so nothing has to be passed in. It costs one lambda per UI
test and no framework magic. On a long-lived suite I would move this into a custom
`BeforeAfterTestAttribute` pair backed by an `AsyncLocal` failure flag so the lambda disappears,
or move to xUnit v3, whose richer test context makes this a non-problem.

Local, container and grid are configuration differences, not code differences:

- **Local:** defaults. `UI__HEADLESS=false` to watch it.
- **Container:** the `Dockerfile` bundles Chrome; `--no-sandbox` and `--disable-dev-shm-usage`
  are already set because both are needed inside Docker.
- **Grid / Selenoid / cloud vendor:** set `UI__REMOTEURL` to the hub. The same options object is
  serialised to the hub instead of starting a local binary — the tests do not change at all.
  For a cloud vendor this is also where vendor capabilities (build name, tunnel, video) get added,
  in the factory and nowhere else.

At scale I would put a per-test `DriverContext` behind an `AsyncLocal<IWebDriver>` so helpers
deep in the stack can reach the current driver without it being threaded through every signature,
and add a health check that discards a driver whose session has died rather than failing the
next test with it.

### Environment configuration and secrets

Precedence, last wins: `appsettings.json` (committed defaults) → `appsettings.{TEST_ENV}.json`
(per-environment overrides) → environment variables.

Pointing the suite at another environment is `TEST_ENV=staging dotnet test`; nothing in the code
changes. Any single value can also be overridden ad hoc with the double-underscore convention
(`API__BASEURL=...`).

Secrets never enter the repository. Locally they come from user-secrets or a gitignored
`appsettings.local.json`; in CI from the secret store, injected as environment variables and
masked in logs. For a real product I would go one step further and pull short-lived credentials
from a vault at run start, so nothing long-lived exists on a CI agent at all. Test accounts are
per-environment and owned by the suite, never shared with manual testers — a human changing a
password should not be able to turn the pipeline red.

### Test independence and parallelism

- **Every test creates the state it needs and owns it.** No test depends on another's side effects
  and no test depends on run order. In this suite that means logging in per test rather than
  reusing a session; on a real product it means seeding a fresh user via API in setup, not
  reusing a shared fixture account that another test might mutate.
- **No shared mutable statics.** Configuration is read-only after load.
- **Parallelism is on by default and capped deliberately.** xUnit runs test *collections* in
  parallel, and by default each test class is its own collection, so classes run concurrently
  while the tests inside one class run in sequence. `xunit.runner.json` caps this at four threads
  — not a technical limit but a resource one: each UI test is a real browser, and oversubscribing
  the agent produces timeouts that look exactly like product bugs.
- **Nothing needs `[Collection]` here**, and that is worth stating rather than leaving implicit.
  A shared collection exists to *serialise* classes that contend over something — a fixed test
  account, a seeded database row, a port. None of these tests contend, so grouping them would
  only cost throughput. The first test that needed a genuinely exclusive resource would get a
  named collection with a `ICollectionFixture` owning that resource's setup and teardown.
- To parallelise *within* a class I would split the class or move to xUnit v3; I would not reach
  for a shared static driver, which is how a suite acquires the flakiness it never recovers from.
- **No `Thread.Sleep` anywhere.** All waiting is conditional, through `WebDriverWait` with
  `NoSuchElement` and `StaleElementReference` ignored, so waits end as soon as the condition
  is true.
- **Data isolation:** where tests must share a data pool, each takes a unique key
  (run id + test name) rather than a magic row that two parallel runs would fight over.

### Reporting, and telling a test problem from a product bug

Every failure should carry enough evidence to be judged without re-running it. Currently captured
automatically: the assertion message (written to name the expectation, not just the values),
a screenshot, the failing URL, and the full request/response log for API calls. For the contract
checks, `Verify.All` and `Verify.ForEach` evaluate every assertion before throwing, so a broken
payload is reported as "six fields missing, here they are" rather than one field per run —
which is the difference between one diagnosis and six round trips. I would add
browser console logs, the har/network log, and a short video for the UI on a long-lived suite.

The triage rule I use:

- **A product bug** reproduces on a second run and reproduces by hand, the screenshot shows the
  application in a genuinely wrong state, and the API response confirms it. It gets a ticket
  against the product.
- **A test problem** is one that passes on retry, fails only in parallel, fails only in CI, or
  whose screenshot shows a perfectly correct page — which usually means a timing assumption or a
  locator tied to something cosmetic. It gets a ticket against the framework, and it is fixed
  rather than retried.

The distinction only stays honest if flakiness is measured. I would track pass rate per test over
time and treat a test that fails intermittently as broken: quarantine it out of the blocking set,
raise a ticket with an owner and a date, and delete it if it is not fixed. A retry mechanism with
no quarantine policy is how a suite quietly stops meaning anything, which is why there is no
blanket retry here.

### CI integration and what the suite may block

The pipeline in `.github/workflows/tests.yml` is staged by cost and by confidence:

| Stage | Trigger | Blocks? |
|---|---|---|
| API tests | every PR and push | **Yes** — fast, deterministic, no browser |
| UI smoke (`Category=Smoke`) | every PR and push, after API | **Yes** — the journeys that must never break |
| UI regression (full) | nightly / manual | **No** — reported, triaged next morning |

Only deterministic tests are allowed to block a merge. A test earns its place in the blocking set
by being stable over a stretch of runs, and loses it the moment it goes flaky. That rule is what
keeps a red pipeline meaningful: if people learn that red sometimes means nothing, they stop
reading it, and the suite has then cost more than it returns.

Everything runs headless in a container, so the agent needs nothing but Docker. Results and
screenshots are uploaded as artifacts on every run, pass or fail.

### First improvements, in order

1. **Response-schema validation** against stored JSON Schemas, so a contract change fails loudly
   at the shape rather than field by field.
2. **A real reporter** (Allure or ReportPortal) with run history, so flakiness is visible as a
   number instead of a feeling — this is the prerequisite for the quarantine policy above.
3. **Split folders into projects** to make the layer boundaries compiler-enforced.
   `Framework.Core` would then have no reference to xUnit at all, which the `ITestLog` seam
   already anticipates.
4. **Test data seeded and torn down via API** in setup, removing the last dependency on
   pre-existing accounts.
5. **A WireMock stub** for one third-party dependency, so the suite can prove our handling of a
   500 or a timeout without waiting for that dependency to have a bad day.
6. **Grid execution in the pipeline** and a cross-browser nightly.

---

## Part 2 — Postman

`postman/JSONPlaceholder.postman_collection.json` and
`postman/JSONPlaceholder.postman_environment.json`.

Both requests use `{{baseUrl}}` from the environment; the host appears nowhere in a URL.
The user id is a `{{userId}}` collection variable for the same reason.

Assertions shared by both requests (status 200, JSON content type, response time) live in the
collection-level test script so they exist once. Each request adds its own: array is non-empty,
every object has the required keys, and no key is empty, `null` or `undefined` — with the index
in the failure message so a failure names the offending element. The users request additionally
checks email format, the nested `address.geo` values and id uniqueness; the posts request
checks that every returned post actually belongs to the requested user, which is the assertion
that would catch a broken filter.

Run from the CLI with:

```bash
newman run postman/JSONPlaceholder.postman_collection.json \
       -e postman/JSONPlaceholder.postman_environment.json
```

## Part 3 — SQL

See `sql/united-package-customers.sql`.

```sql
SELECT DISTINCT
    c.CustomerName,
    c.Country
FROM Customers AS c
INNER JOIN Orders AS o
    ON o.CustomerID = c.CustomerID
INNER JOIN Shippers AS s
    ON s.ShipperID = o.ShipperID
WHERE s.ShipperName = 'United Package'
ORDER BY c.CustomerName;
```

`DISTINCT` because Customers → Orders is one-to-many and a customer with several United Package
orders would otherwise repeat. The shipper is matched by name rather than the hard-coded id `2`,
so the query still reads correctly if the reference data is reordered.
