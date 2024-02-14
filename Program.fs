open System
open Valdi.Library

type Claim =
    { id: Guid
      claimant: string
      location: string
      claimDate: DateTimeOffset }

[<Struct>]
type ClaimError =
    | InvalidLocation of location: string
    | SuspiciousHours of madeAt: TimeOnly
    | SuspiciousWeekDay of dayMade: DayOfWeek
    | TooManyClaims of previousClaims: int

// sample of a type that you don't necessarily own
// but it fits the constrains you want to enforce
type OtherTeamOwnedValidator() =

    member _.Validate claim =

        Validators.validateCases
            [ fun claim ->
                  if claim.claimant = "Alice Smith" then
                      Error(TooManyClaims(10))
                  else
                      Ok claim ]
            claim

module Claim =

    let private invalidLocations (invalidLocations: Set<string>) : SimpleResultFn<Claim, ClaimError> =
        fun claim ->
            if invalidLocations.Contains(claim.location) then
                Error(InvalidLocation(claim.location))
            else
                Ok claim

    let inline private suspiciousHours (start: TimeOnly, ending: TimeOnly) : SimpleResultFn<Claim, ClaimError> =
        fun claim ->
            if claim.claimDate.Hour > start.Hour && claim.claimDate.Hour < ending.Hour then
                TimeOnly.FromTimeSpan(claim.claimDate.TimeOfDay) |> SuspiciousHours |> Error
            else
                Ok claim

    let inline private suspiciousWeekDays (suspiciousDays: Set<DayOfWeek>) : SimpleResultFn<Claim, ClaimError> =
        fun claim ->
            if suspiciousDays.Contains(claim.claimDate.DayOfWeek) then
                SuspiciousWeekDay(claim.claimDate.DayOfWeek) |> Error
            else
                Ok claim

    let private getDefaultValidators () =
        [ invalidLocations (Set.ofSeq [ "Hell"; "Heaven" ])
          suspiciousHours (TimeOnly(02, 0, 0), TimeOnly(05, 0, 0))
          suspiciousWeekDays (
              Set.ofSeq
                  [ DayOfWeek.Monday
                    DayOfWeek.Tuesday
                    DayOfWeek.Wednesday
                    DayOfWeek.Thursday
                    DayOfWeek.Friday ]
          ) ]

    // We can have a factory function that generates a validator depending on
    // the configuration, which is just a list of validation functions
    let getDefaultValidator config =
        { new Validator<Claim, ClaimError> with
            member _.Validate =
                let config = defaultArg config (getDefaultValidators ())

                Validators.validateCases config }

    // Or simply that similarly takes a list of validation functions
    let validate config =
        let config = defaultArg config (getDefaultValidators ())

        fun claim ->
            match Validators.validateCases config claim with
            | Ok validated -> validated, []
            | Error errors -> claim, errors

    // Throw an SRTP function into the mix in this case it is useful to grab external
    // code and make it fit into the SRTP constraint from our own objects
    let inline ofValidator<'Validator when 'Validator: (member Validate: Claim -> Validation<Claim, ClaimError>)>
        (validator: 'Validator)
        =
        { new Validator<Claim, ClaimError> with
            member _.Validate = validator.Validate }


let cases =

    [ { id = Guid.NewGuid()
        claimant = "Jane Doe"
        location = "Heaven"
        claimDate =
          DateTimeOffset.Now
              .AddHours(Random.Shared.Next(0, 24))
              .AddDays(Random.Shared.Next(0, 7)) }
      { id = Guid.NewGuid()
        claimant = "Alice Smith"
        location = "Purgatory"
        claimDate =
          DateTimeOffset.Now
              .AddHours(Random.Shared.Next(0, 24))
              .AddDays(Random.Shared.Next(0, 7)) }
      { id = Guid.NewGuid()
        claimant = "Bob Johnson"
        location = "Limbo"
        claimDate =
          DateTimeOffset.Now
              .AddHours(Random.Shared.Next(0, 24))
              .AddDays(Random.Shared.Next(0, 7)) }
      { id = Guid.NewGuid()
        claimant = "Charlie Brown"
        location = "Nirvana"
        claimDate =
          DateTimeOffset.Now
              .AddHours(Random.Shared.Next(0, 24))
              .AddDays(Random.Shared.Next(0, 7)) }
      { id = Guid.NewGuid()
        claimant = "David Davis"
        location = "Valhalla"
        claimDate =
          DateTimeOffset.Now
              .AddHours(Random.Shared.Next(0, 24))
              .AddDays(Random.Shared.Next(0, 7)) } ]

// IMO simple functions just work equally and given the right design perhaps even more
// flexible than the SRTP constraints, but I think SRTP constrains are very useful when
// you deal with external code I'm no expert in them so that's what I get
cases |> List.map (Claim.validate None) |> List.iter (printfn "%A")

let validator = Claim.getDefaultValidator None

printfn "\n\n"

// See that as a consumer of SRTP stuff, it can be seamless given that we're
// in the SRTP constrains, but as the author, just check the other file, it is
// not the worst thing ever but it clearle requires thinking and a better
// fleshed out design, and I think Interfaces with static abstract members (IWSAMs)
// kind of fit better the dotnet ecosystem, but that opinion is not set in stone yet.
cases
|> List.map (
    Validators.mergeValidators
        [ fun claim ->
              if Random.Shared.Next(0, 20) > 10 then
                  Error(TooManyClaims claim.claimant.Length)
              else
                  Ok claim ]
        [ Claim.ofValidator (OtherTeamOwnedValidator()); validator ]
)
|> List.iter (printfn "%A")
