module Valdi.Library

// Alias for an accumulating error list type
type Validation<'Value, 'Error> = Result<'Value, 'Error list>

// Function aliases, TBH I don't like aliasing functions because
// it makes it harder to discover the function signature
// bug having the extra noise in the signatures is not funny either
type SimpleResultFn<'Value, 'Error> = 'Value -> Result<'Value, 'Error>
type ValidationFn<'Value, 'Error> = 'Value -> Validation<'Value, 'Error>

// An SRTP constraint for a type that has a Validate member
// maybe there's better ways to do this but I'm not there yet
type Validating<'Value, 'Error, 'Validator when 'Validator: (member Validate: ValidationFn<'Value, 'Error>)> =
    'Validator

// A validator interface that fits the SRTP constraint
// keep in mind that this is not required to work with SRTPs
// it is just a handy type to have around and "generalize" the
// validation objects to a common type
type Validator<'Value, 'Error> =
    abstract member Validate: ValidationFn<'Value, 'Error>

module Validators =
    // to be honest we don't really need the SRTP constraint
    // the simple solution is to just act with functions and function signatures for this
    let validateCases<'Value, 'Error>
        (validators: SimpleResultFn<'Value, 'Error> list)
        (value: 'Value)
        : Validation<'Value, 'Error> =
        let rec validateAll' (validators: SimpleResultFn<'Value, 'Error> list) (value: 'Value) (errors: 'Error list) =
            match validators with
            | [] -> if List.isEmpty errors then Ok value else Error errors
            | validator :: rest ->
                match validator value with
                | Ok value' -> validateAll' rest value' errors
                | Error errors' -> validateAll' rest value (errors' :: errors)

        validateAll' validators value []

    // however let's keep the SRTP constraint for the sake of the exercise
    // Most likely I'd like to use those SRTP constraints when I have to work with
    // existing code bases and I don't really want to go back and change the code
    // I'd just want to add a little more of type safety there without adding a new
    // interface on the original type
    let inline mergeValidators<'Value, 'Error, 'Validator when Validating<'Value, 'Error, 'Validator>>
        singleCases
        validators
        value
        =

        let rec validateAll' (validators: 'Validator list) value errors =
            match validators with
            | [] -> if List.isEmpty errors then Ok value else Error errors
            | validator :: rest ->
                match validator.Validate value with
                | Ok value' -> validateAll' rest value' errors
                | Error errors' -> validateAll' rest value (errors' @ errors)

        match validateCases singleCases value with
        | Ok value -> validateAll' validators value []
        | Error errors ->
            match validateAll' validators value [] with
            | Ok _ -> Error errors
            | Error errors' -> Error(errors @ errors')
