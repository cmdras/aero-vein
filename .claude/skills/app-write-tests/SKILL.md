---
name: write-tests
description: Write backend (C#/xUnit) tests in the house style — `<Subject>Should` classes, `<Action>_<Scenario>_<Expected>` methods, Arrange/Act/Assert, fake-it-first, and simulations over mocks. Use when writing or naming API tests in packages/api.tests or packages/api.integration.tests.
---

# Write Tests (C# / xUnit)

The conventions for writing a single backend test well. For the red-green-refactor
*loop* and vertical-slicing discipline, use `Skill('tdd')` — this skill governs what each
test looks like once you sit down to write it.

## Naming

**Test class** — the subject under test plus the `Should` suffix. Reads as a sentence with each method:

```csharp
public class SpacedRepetitionScheduleShould { }   // "Spaced repetition schedule should..."
public class DeckRepositoryShould { }
public class ReviewCardRequestHandlerShould { }
```

**Test method** — `<Action>_<Scenario>_<ExpectedResult>`. The scenario and expected
result are optional only when the action alone is unambiguous; prefer all three.

```csharp
public void Find_ExistingCard_ReturnsCard()
public void Find_MissingCard_ReturnsNull()
public void Schedule_CorrectAnswer_DoublesTheInterval()
public void Schedule_CorrectAnswer_FromTwoDayInterval_ReturnsFour()
public void Review_CardNotInDeck_Throws()
```

Read the method name aloud after the class name: "Deck repository should find existing
card returns card." If it doesn't describe an observable behavior, rename it.

## Structure: Arrange / Act / Assert

Every test has three labelled blocks. Arrange sets up data, Act is one or two lines,
Assert verifies the observable result. No logic (loops, conditionals) in a test.

```csharp
[Fact]
public void Schedule_CorrectAnswer_DoublesTheInterval()
{
    // Arrange
    var schedule = new SpacedRepetitionSchedule(intervalDays: 4);

    // Act
    var next = schedule.OnCorrectAnswer();

    // Assert
    Assert.Equal(8, next.IntervalDays);
}
```

Assert on behavior through the public interface — never reach into private fields or
verify *how* the result was produced. A test that survives an internal refactor is a
good test.

## Fake it first, then triangulate

Getting to green is allowed to be embarrassing. Hardcode the answer to pass the first
test, then add a second data point that forces the real implementation:

```csharp
// First test (interval 4 → 8) passes with a fake:
public SpacedRepetitionSchedule OnCorrectAnswer() => new(intervalDays: 8);   // hardcoded — green!

// A second data point (Schedule_CorrectAnswer_FromTwoDayInterval_ReturnsFour,
// interval 2 → 4) makes the hardcoded 8 impossible, forcing the real rule:
public SpacedRepetitionSchedule OnCorrectAnswer() => new(intervalDays: IntervalDays * 2);
```

When the implementation is obvious (`x + y`), just write it. When it isn't, triangulate
with a second example rather than guessing — and take that as a signal your steps are too big.

## Simulations over mocks

For dependencies **you control** (repositories, stores), do not mock. Mocks bloat the test
with setup, couple it to the collaborator's internals, and test implementation instead of
behavior. Write a simple in-memory simulation that has the same behavior as the real thing:

```csharp
public sealed class InMemoryDeckRepository : IDeckRepository
{
    private readonly List<Deck> _decks;
    public InMemoryDeckRepository(IEnumerable<Deck> seed) => _decks = seed.ToList();

    public Deck? Find(int id) => _decks.FirstOrDefault(d => d.Id == id);
    public void Add(Deck deck) => _decks.Add(deck);
}
```

Reserve real mocking (Moq) for things you *don't* control — third-party services, external
APIs — where a simulation isn't practical.

### Prove the simulation matches reality

A simulation is only trustworthy if it behaves like the real implementation. Put the tests
in an abstract base class and run the **same suite** against both the in-memory and the real
(database-backed) repository:

```csharp
public abstract class DeckRepositoryShould
{
    protected abstract IDeckRepository CreateSut(IEnumerable<Deck> seed);

    [Fact]
    public void Find_ExistingDeck_ReturnsDeck()
    {
        // Arrange
        var spanish = Decks.Spanish.Build();
        var sut = CreateSut([spanish]);

        // Act
        var found = sut.Find(spanish.Id);

        // Assert
        Assert.Equal(spanish, found);
    }
}

// Unit project (packages/api.tests) — fast, in-memory:
public class InMemoryDeckRepositoryShould : DeckRepositoryShould
{
    protected override IDeckRepository CreateSut(IEnumerable<Deck> seed)
        => new InMemoryDeckRepository(seed);
}

// Integration project (packages/api.integration.tests) — real backing store:
public class SqlDeckRepositoryShould : DeckRepositoryShould
{
    protected override IDeckRepository CreateSut(IEnumerable<Deck> seed) =>
        /* build a SqlDeckRepository against a test database, seeded */;
}
```

Behavior-level tests live in the unit project against the simulation; the integration
project re-runs the contract against the real store to guarantee parity.

## Test data builders

Keep Arrange blocks readable with named builders rather than inline object construction
repeated across tests:

```csharp
var spanish = Decks.Spanish.Build();
var card = Cards.Default.WithInterval(4).Build();
```

A builder gives a sensible default and lets each test override only the field it cares
about — so the test documents *what matters* for that case.

## Which project?

- **packages/api.tests** — behavior and logic against simulations. Fast, no I/O. (`bun run test:api:unit`)
- **packages/api.integration.tests** — the same contracts against real infrastructure, plus endpoint tests via `WebApplicationFactory`. (`bun run test:api:int`)

## Checklist per test

```
[ ] Class named <Subject>Should
[ ] Method named <Action>_<Scenario>_<Expected>, reads as a behavior
[ ] Arrange / Act / Assert blocks labelled
[ ] Asserts on observable behavior through the public interface
[ ] No logic in the test body
[ ] Owned dependencies use a simulation, not a mock
[ ] Simulation has a shared contract test against the real implementation
```
