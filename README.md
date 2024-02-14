> **_NOTE_**: Please take this as an example and not something actually useful. There are better ways to do validation in F#
>
> - [FsToolkit.ErrorHandling](https://demystifyfp.gitbook.io/fstoolkit-errorhandling) (Totally Recommended, not just for validation)
> - [Validus](https://github.com/pimbrouwers/Validus)
>
> And there might be others around but these two are the ones that come to my mind and I've used in the past specially the first one
> That can be used to more than just validation.

## What is this?

This is me, being bored and looking to play with SRTPs and trying to come up with a solution to a recent problem I faced a few days ago.

## What is the problem?

Create a simple mini validation system that can be used to apply arbitrary validation rules to a given type which will return a given error for that type.

It should be configurable and easy to use.

## What is the solution?

There was no solution, as it was an arbitrary problem to solve, you can come up with different ideas methods paradigms and anything to solve this problem.

My take here was to go as simple as possible but also mix in SRTP constraints to enable use cases where we can use external code that represents "existing code not built from or for us" but with a little bit of help it can fit our validation system.

The ideal solution for me without diving too much into _functional programming_ would be to take a `'Value` type and apply a `Validator` to it, and if it fails, return an error.

`'Value -> Result<'Value, 'Error>`

This can be further extended to a `Validation<'Value, 'Error>` type that is just an alias to `Result<'Value, 'Error list>` that can accumulate errors on a single value.

That way it is simple to create separate validation functions that can be applied sequentially or cumulative to a value and return a list of errors.

e.g.:

```fsharp
let validate<'Value, 'Error>
    (validators: ('Value -> Result<'Value, 'Error>) list)
    (value: 'Value)
    : Validation<'Value, 'Error> = ...

value |> validate [validator1; validator2; validator3]
```

And so on.

## Conclusions

Even if you're not prepared for "something" there are certain situations that happen to be similar, in this case we were talking about validation, but as long as you're able to "generalize" or abstract problems into generic solutions, be sure that you'll be ready for "something" even if you're not prepared for it.

My problem could have been something like abstracting logic from a function that transforms an object into a more generalized pattern, and interestingly enough the solution could be the similar, create a set of functions that take a value, transform it and return a new value either wrapped in a `Result` or other more suitable type, in the end these functions could compose very well if designed correctly.
