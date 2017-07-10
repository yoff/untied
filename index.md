- title : Untied fixed points
- description : A hands-ons introduction to fixed point semantics
- author : Rasmus Lerchedahl Petersen
- theme : night
- transition : default

***

## Untied Fixed Points

<br />
<br />

### A hands-ons introduction to fixed point semantics

<br />
<br />
Rasmus Lerchedahl Petersen - [@yoff](https://yoff.github.io)

---
> "When we define a recursive function, we are asking the compiler to compute a fixed point"

***
### Fixed points

Q:
<br />
 What is a fixed point?

---
A:
<br />
 A point that is not moved
<br />
(by some function)

---
Formally:
<br />
$x$ is a fixed point for $f$ iff
$$ f(x) = x $$

---
#### Example

<svg id="fixdiag" xmlns="http://www.w3.org/2000/svg" width="330" height="450">
    <g transform="translate(10,10), scale(3) ">
        <line x1="0" y1="50" x2="100" y2="50" style="stroke:white" />
        <line x1="50" y1="0" x2="50" y2="100" style="stroke:white" />
        <line x1="0" y1="100" x2="0" y2="100" style="stroke:grey">
            <animate
                attributeName="x2"
                begin="fixdiag.click"
                from="0"
                to="100"
                dur=".7s"
                fill="freeze"
            />
            <animate
                attributeName="y2"
                begin="fixdiag.click"
                from="100"
                to="0"
                dur=".7s"
                fill="freeze"
            />
        </line>
        <path
            style="fill:none; stroke:green"
            d="M 0 70
            Q 10 70 20 80
            Q 40 100 80 20
            Q 90 0 100 0"
        />
        <circle cx="20" cy="80" r="0" style="fill:blue; stroke:lightblue">
            <animate
                attributeName="r"
                begin="fixdiag.click+1"
                from="0"
                to="2"
                dur=".5s"
                fill="freeze"
            />
        </circle>
        <circle cx="80" cy="20" r="0" style="fill:blue; stroke:lightblue">
            <animate
                attributeName="r"
                begin="fixdiag.click+1.2"
                from="0"
                to="2"
                dur=".5s"
                fill="freeze"
            />
        </circle>
        <circle cx="100" cy="0" r="0" style="fill:blue; stroke:lightblue">
            <animate
                attributeName="r"
                begin="fixdiag.click+1.4"
                from="0"
                to="2"
                dur=".5s"
                fill="freeze"
            />
        </circle>
    </g>
    <text x="130" y="350" style="fill:green; stroke:none">
        y = f(x)
    </text>
    <text x="130" y="500" style="fill:grey; stroke:none">
            <animate
                attributeName="y"
                begin="fixdiag.click"
                from="500"
                to="390"
                dur="1s"
                fill="freeze"
            />
        y = x
    </text>
    <text x="130" y="540" style="fill:lightblue; stroke:none">
            <animate
                attributeName="y"
                begin="fixdiag.click+1"
                from="540"
                to="430"
                dur="1s"
                fill="freeze"
            />
        x = f(x)
    </text>
</svg>

***
### Recursive functions

Q:
<br />
 What is a recursive function?

---
A:
<br />
 A function that calls itself

---
Actually:
<br />
 The solution to a functional equation
in which the function to solve for appears on both sides
<br />
<br />
Usually of the form
$$f(x) = e[f,x]$$

---
#### Example

$f(x) = f(x-2) + f(x-1)$

---
#### More complete

<!--$$
\begin{aligned}
f(0) &\ =\ 0 \\
f(1) &\ =\ 1 \\
f(x) &\ =\ f(x-2) + f(x-1)
\end{aligned}
$$-->

$$
f(x) =
\begin{cases}
\ 0 & ,\ x = 0 \\
\ 1 & ,\ x = 1 \\
\ f(x-2) + f(x-1) & ,\ x > 1 
\end{cases}
$$

***
### Solutions are fixed points

The right hand side can be viewed as a functional
$$e: (\mathbb{N} \rightarrow \mathbb{N}) \rightarrow (\mathbb{N} \rightarrow \mathbb{N})$$
<br />
$$e(f)\ =\quad x \mapsto\ f(x-2) + f(x-1)$$

--- 
#### Fixed point
$f$ is a fixed point for $e$ iff
$$e(f) = f$$
<br />
That is
$$f = e(f) = f(x-2) + f(x-1)$$

---
#### The fixed point combinator
$x$ is a fixed point for $f$ iff
$$f(x) = x$$
<br />
So a fixed point combinator must satisfy
$$f(\ fix(f)\ ) = fix(f)$$
<br />
We can use the left hand side as a definition

    let rec fix f x = f (fix f) x

***
### Coding without recursion
So, when we write

    let rec fib x =
      if x < 2 then x
      else fib (x-2) + fib (x-1)

we are asking the compiler to compute the fixed point of

$$
e(f) = x \mapsto\
\begin{cases}
\ 0 & ,\ x = 0 \\
\ 1 & ,\ x = 1 \\
\ f(x-2) + f(x-1) & ,\ x > 1 
\end{cases}
$$

---
$$
e(f) = x \mapsto\
\begin{cases}
\ 0 & ,\ x = 0 \\
\ 1 & ,\ x = 1 \\
\ f(x-2) + f(x-1) & ,\ x > 1 
\end{cases}
$$

We can write this function explicitly

    let fibU fibR x =
      if x < 2 then x
      else fibR (x-2) + fibR (x-1)

and then define `fib` as the fixed point

    let fib = fix fibU

---
#### Does this work?
With normal recursion

$$
\begin{aligned}
    fib\ 3 & \mapsto fib\ 2 + fib\ 1 \\
           & \mapsto (fib\ 1 + fib\ 0) + 1 \\
           & \mapsto (1 + 0) + 1 \\
           & \mapsto 2 
\end{aligned}
$$

---
With explicit fixed point

$$
\begin{aligned}
    fib\ 3 & \mapsto \underline{fix\,\, fibU\ 3} \\
           & \mapsto fibU\ (fix\,\, fibU)\ 3 \\
           & \mapsto (\underline{fix\,\, fibU\ 2}) + (\underline{fix\,\, fibU\ 1}) \\
           & \mapsto (fibU\ (fix\,\, fibU)\ 2) + (fibU\ (fix\,\, fibU)\ 1) \\
           & \mapsto ((\underline{fix\,\, fibU\ 1}) + (\underline{fix\,\, fibU\ 0})) + 1 \\
           & \mapsto ((fibU\ (fix\,\, fibU)\ 1) + (fibU\ (fix\,\, fibU)\ 0)) + 1 \\
           & \mapsto (1 + 0) + 1 \\
           & \mapsto 2 
\end{aligned}
$$

***
### Try it now
- Hands-on
    * [F# script](https://github.com/yoff/untied/blob/master/untied.fs)
    * [Solutions](https://github.com/yoff/untied/blob/master/untied_solutions.fs)
- Online ways to try
    * http://fable.io/repl
    * http://www.tryfsharp.org/Create

***
### Links
- Blog post
    * https://eiriktsarpalis.wordpress.com/2013/02/13/parametric-open-recursion-pt-1/
    * https://eiriktsarpalis.wordpress.com/2013/02/14/parametric-open-recursion-pt-2/
- Code
    * https://github.com/rgrig/barista
    * (see https://github.com/rgrig/barista/blob/master/src/common/utils.ml#L208)
- Video
    * https://www.youtube.com/watch?v=4DdUEHOhAB4
- Presentation
    * https://yoff.github.io/untied
