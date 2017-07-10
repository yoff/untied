// Accompagnying slides are at https://yoff.github.io

// We will illustrate the concept of untied fixed points,
// through a number of small functions.
// You are meant to run the code snippets and laugh with glee and surprise.


/// Standard
// Suppose we want to implement the Fibonacci sequence (who doesn't).
// We might write a standard implementation like so:
let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)
[0..5] |> List.map fib

// Now suppose we wish to log all the function calls (who doesn't),
// we can easily accomplish this:
let rec fibL n =
  printfn "Called with %A" n
  if n < 2 then n else fibL (n-1) + fibL (n-2)

// This works beautifully
fibL 5

// We may, however, prefer not to add such a line of code to all the
// functions we wish to log. Rather, we would separate concerns and
// define the logging logic on the side:
let log f x =
  printfn "Called with %A" x
  f x

// We can then construct our function-with-logging via composition:
let fibL = log fib

// Excellent, we did not have to touch the definition of fib and we
// now have a logging version. Except...
fibL 5


/// Untied
// We need more control over recursive calls. Specifically, we need to
// be able to insert the logging at all (recursive) calls. We will achieve
// this by inserting the loggin _before_ the fixed point is tied.

// So we need our own fixed point combinator.
let rec fix fU x = fU (fix fU) x
// We will use the convention then fU denotes the untied version of f
// while fR denotes the recursive call.

// The untied version of fib is exactly like the tied version, except
// recursive calls are made to an explicit argument (and so the fuction is
// no longer recursive).
let fibU fibR n = if n < 2 then n else fibR (n-1) + fibR (n-2)

// We can now tie the fixed point
let fib = fix fibU

// and verify that we have recovered the original function
[0..5] |> List.map fib

// Logging is now written as a functional, which transforms one untied
// function into a new one. Here, it basically looks the same as before
// (except we have parameterised the logger, to have fun later).
let log logger fU fR x =
  logger x
  fU fR x

// Here is the simpe logger from before
let printIt x = printfn "Called with %A" x

// We can now add logging to fibU before tying the fixed point,
// without touching the definition of fibU.
let fibL = fibU |> log printIt |> fix

fibL 5


/// Interlude
// Since the logging is nicely separated from the function, we can
// get fancy with it. To start with, let us log to a ref cell so we
// have the values available rather than on screen.
let refLogger refCell x =
  refCell := x :: !refCell

// Create a ref cell and a logger for it
let store : int list ref = ref []
let intLogger = refLogger store

// We can now build a function with logging
let fibStore = fibU |> log intLogger |> fix

// Call it and check the log
fibStore 5
!store

// Now that we have a list of logged values, let us turn it into an animation
// The following function produces a string in the svg format
let intLogToSVG ints =
  let width = 100
  let offset = 10
  let normBy c n = (float n) / (float c)
  let nKeys =
    ints |> List.length
  let keyTimes =
    [0..nKeys-1]
    |> List.map (normBy (nKeys-1) >> sprintf "%f")
    |> String.concat "; "
    |> sprintf "keyTimes=\"%s\""
  let maxValue = ints |> Seq.max
  let minValue = ints |> Seq.min
  let range = maxValue - minValue
  let normalise v =
    width * (v + minValue) |> normBy range |> (+) (float offset)
  let keyValues =
    ints
    |> List.map (normalise >> sprintf "%f")
    |> String.concat "; "
    |> sprintf "values=\"%s\""
  let duration = nKeys / 4
  let attributes =
    [ "attributeName=\"cx\""
      "begin=\"0s\""
      sprintf "dur=\"%ds\"" duration
      keyTimes
      keyValues
      "fill=\"freeze\""
    ]
  let attr =
    attributes
    |> String.concat "\n      "
    |> sprintf "      %s"
  let zero = normalise 0
  let tick n =
    let x = normalise n
    [ sprintf "  <line x1=\"%f\" y1=\"10\" x2=\"%f\" y2=\"40\" style=\"stroke:blue\" />" x x
      sprintf "  <text x=\"%f\" y=\"52\">%d</text>" (x-4.0) n
    ]
  [ yield "<svg xmlns=\"http://www.w3.org/2000/svg\">"
    yield! [minValue ..maxValue] |> List.collect tick
    yield "  <circle cx=\"40\" cy=\"30\" r=\"5\">"
    yield "    <animate"
    yield attr
    yield "    />"
    yield "  </circle>"
    yield "</svg>"
  ]
  |> String.concat "\n"
  |> sprintf "%s"
(* example svg
<svg xmlns="http://www.w3.org/2000/svg">
  <circle cx="40" cy="30" r="5">
    <animate
      attributeName="cx"
      begin="0s"
      dur="1s"
      keyTimes="0;0.5;1"
      values="40;50;85"
    />
  </circle>
</svg>
*)

// This code saves the string to disk
let localPath = __SOURCE_DIRECTORY__
let svgPath fileName = System.IO.Path.Combine(localPath,fileName)
let saveSVG fileName svg = System.IO.File.WriteAllText (svgPath fileName, svg)

// We can now render our stored values to an svg file
!store |> List.rev |> intLogToSVG |> saveSVG "fib.svg"
// At this point, you should have a file called fib.svg in you local folder.
// Try opening it it a browser.

// It is not so easy to distinguish the values, let us change that
let toPairs l = l |> Seq.pairwise |> Seq.toList
let flattenPairs ps = ps |> List.collect (fun (a,b) -> [a; b])
!store |> List.rev |> toPairs |> flattenPairs |> intLogToSVG |> saveSVG "fib1.svg"


/// Memoization
// Storing argument values can be useful for more than logging.
// A common trick is to store arguments together with computed values.
// That way, later calls may not need to compute if they can just look up the result.

// Here is a functional that adds memoization using a dictionary
let memo fU =
  let store = System.Collections.Generic.Dictionary<'K,'V>()
  let lookup k =
    match store.TryGetValue k with
    | true, v -> Some v
    | false, _ -> None
  let record k v =
    store.Add (k, v)
  fun fR x ->
    match lookup x with
    | Some v -> v // we already know the answer
    | None ->     // we do not know
      let v = fU fR x // compute the answer
      record x v      // record the computed value
      v
      
// Let us try it out
let fibM = fibU |> memo |> fix
[0..5] |> List.map fibM

// Not much to see unless we do timing experiments,
// luckily, we have the tools to see what happens
let fibML = fibU |> memo |> log printIt |> fix

[0..5] |> List.map fibML  // try running this more than once

let fibLM = fibU |> log printIt |> memo |> fix

[0..5] |> List.map fibLM  // try running this more than once

// Questions: What is the difference between fibML and fibLM?

// Exercise: Compare this memoization with a functional that
// adds memoization to a normal recursive function.

// Exercise: You have now seen both how to add logging and how to
// add some state (the dictionary). Try to implement a logger which
// indents the log by the current level of recursion

// <Solution>
// This may not be the most obvious solution
// We first crete a functional that tags the calls with the recursion level
let tagLevel fU fR (n,x) = fU (fun xR -> fR (n+1,xR)) x

// Then we create a logger that uses the level
let indent n = String.replicate n " "
let logLevel (n,x) =
  printfn "%sCalled with: %A" (indent n) x

// We can now build the function
let fibLi = fibU |> tagLevel |> log logLevel |> fix
// We have to call it with the level
fibLi (0, 5)

// It may be useful to be able to set the level, but in most cases it just feels clunky
let initWith v f x = f (v,x)
let fibLi = fibU |> tagLevel |> log logLevel |> fix |> initWith 0
fibLi 5
// </Solution>


/// Bounded recursion
// Another technique for optimising is to switch algorithm once a certain criterion,
// such as the input getting small, is met.

// Here is a functional that switches to a base function on small inputs
let bound n fBase fU =
  fun fR x ->
    if x <= n then fBase x
    else fU fR x 

// We can approximate the Fibonacci function with the identity
let fibBase x = x

// Here we can actually get away with no base case
let fibUnb fibR x = fibR (x-2) + fibR (x-1) 

// We can recover the standard sequence
let fib1 = fibUnb |> bound 1 fibBase |> fix
[0..7] |> List.map fib1

// Or get a faster but more approximate one
let fib4 = fibUnb |> bound 4 fibBase |> fix
[0..7] |> List.map fib4
// might still be ok for computing the golden ratio?

// Exercise: switch from mergesort to insertion on small lists
// <Solution>
// We will just write insertion sort in the regular way for now
// But if we did an untied version, we could add a specific logger
let insertionSort l =
  // assume post is sorted
  let rec insertLoop pre x post =
    match post with
    | [] -> // ran out of list
      x::pre |> List.rev
    | p::ps when p < x -> // not yet there
      insertLoop (p::pre) x ps
    | _ -> // found the place
      List.rev pre @ (x::post)
  let insert = insertLoop []

  let rec loop sorted unsorted =
    match unsorted with
    | [] -> // done
      List.rev sorted
    | x::xs -> // find place for x and carry on
      loop (insert x sorted) xs

  loop [] l |> List.rev

// Here is a functional that switches algorithm based on a predicate
let switch pred fBase fU =
  fun fR x ->
    if pred x then
      printfn "Calling base function"
      fBase x
    else fU fR x 

// The predicate to switch on
let isSmall n l = List.length l < n

let mergeSortU mergeSortR l =
  let rec mergeLoop acc left right =
    match left, right with
    | [], _ ->
      (List.rev acc)@right
    | _, [] ->
      (List.rev acc)@left
    | x::xs, y::ys when x < y ->
      mergeLoop (x::acc) xs right
    | x::xs, y::ys ->
      mergeLoop (y::acc) left ys
  let merge = mergeLoop []

  match List.length l with
  | 0 | 1 -> l
  | n ->
    let n2 = (n+1) / 2
    let left, right = List.take n2 l, List.skip n2 l
    merge (mergeSortR left) (mergeSortR right)

// We make a type annotation to get around value restriction
type Sorter<'a> = 'a list -> 'a list

// Here we could try out different values, even if we had access to neither
// sorting implementation
let mergeSortB : Sorter<int> = mergeSortU |> switch (isSmall 16) insertionSort |> fix
mergeSortB [8; 3; 9; 11; -121; 4; 4; 23; 3; 4; -14; -23; 0; 17; 71; 16; 17; 18]
// </Solution>


/// Tracking
// Take a moment to consider what the following functional does
let successive fU fR (x, y) = fU (fun z -> fR (y, z)) y

// This one earned me the comment "How do you program with these things?"

// We can motivate it with our animation generator.
// Suppose we want the dot to move from the value that caused a call to the
// argument value of the call.abs
// We could try to take the log from before and do Seq.pairwise, but it would not be quite right.

// Used to initialise the first 'previous' value
let diag f x = f (x,x)

// This shows which call caused the current call
let fibSu = fibU |> successive |> log printIt |> fix |> diag
fibSu 5

// Why not render it
let pairStore : (int * int) list ref = ref []
let intPairLogger = refLogger pairStore
let fibStoreSu = fibU |> successive  |> log intPairLogger |> fix |> diag
fibStoreSu 5

!pairStore |> List.rev |> flattenPairs |> intLogToSVG |> saveSVG "fib2.svg"


/// Go explore!
// Ideas:
// - use some of the functionals on your favourite recursive functions
// - consider other functoinals to write
// - create dot-file of calls
// - animated rewrites of lambda terms?
// - consider mutual recursion (see Eirik's blog)
