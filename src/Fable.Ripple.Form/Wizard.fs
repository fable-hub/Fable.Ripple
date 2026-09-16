namespace Fable.Ripple.Form

open Fable.Ripple

/// <summary>
/// One page of a wizard: its title and its form with the output erased.
/// </summary>
[<NoComparison; NoEquality>]
type Step =
    {
        Title: string
        Form: Form<unit>
    }

/// <summary>
/// A form split into steps.
///
/// Composes like a form: <see cref="M:Fable.Ripple.Form.Wizard.succeed"/> takes the function that builds the
/// output and <see cref="M:Fable.Ripple.Form.Wizard.step"/> feeds it one step at a time.
/// </summary>
[<NoComparison; NoEquality>]
type Wizard<'Output> =
    {
        /// <summary>
        /// The typed output of all steps: the <see cref="M:Fable.Ripple.Form.Base.append"/> of every step.
        /// </summary>
        Form: Form<'Output>
        /// <summary>
        /// Tracked read of the present steps; <see cref="M:Fable.Ripple.Form.Wizard.stepIf"/> and
        /// <see cref="M:Fable.Ripple.Form.Wizard.andThen"/> add steps that depend on values.
        /// </summary>
        Steps: unit -> Step list
    }

[<RequireQualifiedAccess>]
module Wizard =

    /// <summary>
    /// A wizard with no step whose output is <paramref name="output"/>.
    /// </summary>
    /// <param name="output">The value the wizard yields, usually the function that builds it from the steps</param>
    /// <returns>A wizard with no step</returns>
    let succeed (output: 'Output) : Wizard<'Output> =
        {
            Form = Base.succeed output
            Steps = fun () -> []
        }

    /// <summary>
    /// Appends the steps of a wizard to another one and feeds its output to the function the other one yields.
    /// </summary>
    /// <param name="inner">The wizard to append</param>
    /// <param name="wizard">The wizard to append to, whose output is a function</param>
    /// <returns>A wizard with the steps of both</returns>
    let append (inner: Wizard<'A>) (wizard: Wizard<'A -> 'B>) : Wizard<'B> =
        {
            Form = Base.append inner.Form wizard.Form
            Steps = fun () -> wizard.Steps() @ inner.Steps()
        }

    /// <summary>
    /// A wizard made of one step.
    /// </summary>
    /// <param name="title">The step's title</param>
    /// <param name="form">The step's form</param>
    /// <returns>A wizard whose output is the form's</returns>
    let single (title: string) (form: Form<'A>) : Wizard<'A> =
        let step =
            {
                Title = title
                Form = Base.map ignore form
            }

        {
            Form = form
            Steps = fun () -> [ step ]
        }

    /// <summary>
    /// Appends one step and feeds its output to the function the wizard yields.
    /// </summary>
    /// <param name="title">The step's title</param>
    /// <param name="form">The step's form</param>
    /// <param name="wizard">The wizard to append to</param>
    /// <returns>A wizard with one more step</returns>
    let step (title: string) (form: Form<'A>) (wizard: Wizard<'A -> 'B>) : Wizard<'B> =
        append (single title form) wizard

    /// <summary>
    /// Appends a step present only while a condition holds.
    ///
    /// The step's output is <c>None</c> while it is absent. Its fields keep their values.
    /// </summary>
    /// <param name="condition">Tracked read of the condition</param>
    /// <param name="title">The step's title</param>
    /// <param name="form">The step's form</param>
    /// <param name="wizard">The wizard to append to</param>
    /// <returns>A wizard with one more step while the condition holds</returns>
    let stepIf
        (condition: unit -> bool)
        (title: string)
        (form: Form<'A>)
        (wizard: Wizard<'A option -> 'B>)
        : Wizard<'B>
        =
        let shown = Signal.computed condition
        let conditional = Base.showIf (fun () -> shown.Value) form

        let inner =
            {
                Form = conditional
                Steps =
                    fun () ->
                        if shown.Value then
                            [
                                {
                                    Title = title
                                    Form = Base.map ignore form
                                }
                            ]
                        else
                            []
            }

        append inner wizard

    /// <summary>
    /// Transforms the output of a wizard.
    /// </summary>
    /// <param name="fn">Applied to the output</param>
    /// <param name="wizard">The wizard whose output is transformed</param>
    /// <returns>A wizard with the same steps</returns>
    let map (fn: 'A -> 'B) (wizard: Wizard<'A>) : Wizard<'B> =
        {
            Form = Base.map fn wizard.Form
            Steps = wizard.Steps
        }

    /// <summary>
    /// Adds the steps of a child wizard after the ones of a wizard, built from its output.
    ///
    /// The child is built again when the output changes. Apply it to a wizard whose output is only the value
    /// the next steps depend on, then <see cref="M:Fable.Ripple.Form.Wizard.append"/> the result, so typing
    /// elsewhere does not rebuild the steps.
    /// </summary>
    /// <param name="child">Builds the child wizard from the output</param>
    /// <param name="wizard">The wizard whose output the child depends on</param>
    /// <returns>A wizard with the steps of both, whose output is the child's</returns>
    let andThen (child: 'A -> Wizard<'B>) (wizard: Wizard<'A>) : Wizard<'B> =
        let form, current =
            Base.andThenWith child (fun (child: Wizard<'B>) -> child.Form) wizard.Form

        {
            Form = form
            Steps =
                fun () ->
                    wizard.Steps()
                    @ (
                        match current.Value with
                        | Some child -> child.Steps()
                        | None -> []
                    )
        }
