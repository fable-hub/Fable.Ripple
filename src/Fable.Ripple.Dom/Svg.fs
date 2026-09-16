namespace Fable.Ripple.Dom

open Fable.Ripple
open Base

/// SVG elements. Created in the SVG namespace (`createElementNS`) so they render as
/// real SVG. Mix in `attr.*` (setAttribute-based, e.g. `attr.id`, `attr.className`)
/// and `svgAttr.*` (presentation attributes) freely - both produce `DomItem`.
type Svg =

    /// Generic SVG element by tag name.
    static member elem (tag: string) (items: DomItem list) : DomItem =
        createElementNS svgNamespace tag items

    (*
        Structure
    *)
    static member svg(items: DomItem list) : DomItem =
        createElementNS svgNamespace "svg" items

    static member g(items: DomItem list) : DomItem = createElementNS svgNamespace "g" items

    static member defs(items: DomItem list) : DomItem =
        createElementNS svgNamespace "defs" items

    static member symbol(items: DomItem list) : DomItem =
        createElementNS svgNamespace "symbol" items

    static member use'(items: DomItem list) : DomItem =
        createElementNS svgNamespace "use" items

    static member image(items: DomItem list) : DomItem =
        createElementNS svgNamespace "image" items

    static member switch(items: DomItem list) : DomItem =
        createElementNS svgNamespace "switch" items

    static member foreignObject(items: DomItem list) : DomItem =
        createElementNS svgNamespace "foreignObject" items

    static member marker(items: DomItem list) : DomItem =
        createElementNS svgNamespace "marker" items

    static member view(items: DomItem list) : DomItem =
        createElementNS svgNamespace "view" items

    static member a(items: DomItem list) : DomItem = createElementNS svgNamespace "a" items

    static member title(items: DomItem list) : DomItem =
        createElementNS svgNamespace "title" items

    static member desc(items: DomItem list) : DomItem =
        createElementNS svgNamespace "desc" items

    static member metadata(items: DomItem list) : DomItem =
        createElementNS svgNamespace "metadata" items

    (*
        Shapes
    *)
    static member circle(items: DomItem list) : DomItem =
        createElementNS svgNamespace "circle" items

    static member ellipse(items: DomItem list) : DomItem =
        createElementNS svgNamespace "ellipse" items

    static member line(items: DomItem list) : DomItem =
        createElementNS svgNamespace "line" items

    static member rect(items: DomItem list) : DomItem =
        createElementNS svgNamespace "rect" items

    static member path(items: DomItem list) : DomItem =
        createElementNS svgNamespace "path" items

    static member polygon(items: DomItem list) : DomItem =
        createElementNS svgNamespace "polygon" items

    static member polyline(items: DomItem list) : DomItem =
        createElementNS svgNamespace "polyline" items

    (*
        Text
    *)
    static member text(items: DomItem list) : DomItem =
        createElementNS svgNamespace "text" items

    static member tspan(items: DomItem list) : DomItem =
        createElementNS svgNamespace "tspan" items

    static member textPath(items: DomItem list) : DomItem =
        createElementNS svgNamespace "textPath" items

    static member tref(items: DomItem list) : DomItem =
        createElementNS svgNamespace "tref" items

    (*
        Paint servers
    *)
    static member linearGradient(items: DomItem list) : DomItem =
        createElementNS svgNamespace "linearGradient" items

    static member radialGradient(items: DomItem list) : DomItem =
        createElementNS svgNamespace "radialGradient" items

    static member stop(items: DomItem list) : DomItem =
        createElementNS svgNamespace "stop" items

    static member pattern(items: DomItem list) : DomItem =
        createElementNS svgNamespace "pattern" items

    static member clipPath(items: DomItem list) : DomItem =
        createElementNS svgNamespace "clipPath" items

    static member mask(items: DomItem list) : DomItem =
        createElementNS svgNamespace "mask" items

    (*
        Filters
    *)
    static member filter(items: DomItem list) : DomItem =
        createElementNS svgNamespace "filter" items

    static member feBlend(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feBlend" items

    static member feColorMatrix(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feColorMatrix" items

    static member feComponentTransfer(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feComponentTransfer" items

    static member feComposite(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feComposite" items

    static member feConvolveMatrix(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feConvolveMatrix" items

    static member feDiffuseLighting(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feDiffuseLighting" items

    static member feDisplacementMap(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feDisplacementMap" items

    static member feDistantLight(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feDistantLight" items

    static member feDropShadow(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feDropShadow" items

    static member feFlood(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feFlood" items

    static member feFuncA(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feFuncA" items

    static member feFuncB(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feFuncB" items

    static member feFuncG(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feFuncG" items

    static member feFuncR(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feFuncR" items

    static member feGaussianBlur(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feGaussianBlur" items

    static member feImage(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feImage" items

    static member feMerge(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feMerge" items

    static member feMergeNode(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feMergeNode" items

    static member feMorphology(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feMorphology" items

    static member feOffset(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feOffset" items

    static member fePointLight(items: DomItem list) : DomItem =
        createElementNS svgNamespace "fePointLight" items

    static member feSpecularLighting(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feSpecularLighting" items

    static member feSpotLight(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feSpotLight" items

    static member feTile(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feTile" items

    static member feTurbulence(items: DomItem list) : DomItem =
        createElementNS svgNamespace "feTurbulence" items

    (*
        Animation
    *)
    static member animate(items: DomItem list) : DomItem =
        createElementNS svgNamespace "animate" items

    static member animateMotion(items: DomItem list) : DomItem =
        createElementNS svgNamespace "animateMotion" items

    static member animateTransform(items: DomItem list) : DomItem =
        createElementNS svgNamespace "animateTransform" items

    static member mpath(items: DomItem list) : DomItem =
        createElementNS svgNamespace "mpath" items

    static member set'(items: DomItem list) : DomItem =
        createElementNS svgNamespace "set" items

/// SVG presentation attributes. Static by default; make any of them reactive with
/// the generic `svgAttr.attr` overloads (a `Signal<string>` or a `unit -> string`).
type svgAttr =

    (*
        Geometry (numeric or string)
    *)
    static member x(v: float) : DomItem = attribute "x" (string v)
    static member x(v: int) : DomItem = attribute "x" (string v)
    static member x(v: string) : DomItem = attribute "x" v
    static member y(v: float) : DomItem = attribute "y" (string v)
    static member y(v: int) : DomItem = attribute "y" (string v)
    static member y(v: string) : DomItem = attribute "y" v
    static member x1(v: float) : DomItem = attribute "x1" (string v)
    static member x1(v: int) : DomItem = attribute "x1" (string v)
    static member y1(v: float) : DomItem = attribute "y1" (string v)
    static member y1(v: int) : DomItem = attribute "y1" (string v)
    static member x2(v: float) : DomItem = attribute "x2" (string v)
    static member x2(v: int) : DomItem = attribute "x2" (string v)
    static member y2(v: float) : DomItem = attribute "y2" (string v)
    static member y2(v: int) : DomItem = attribute "y2" (string v)
    static member cx(v: float) : DomItem = attribute "cx" (string v)
    static member cx(v: int) : DomItem = attribute "cx" (string v)
    static member cx(v: string) : DomItem = attribute "cx" v
    static member cy(v: float) : DomItem = attribute "cy" (string v)
    static member cy(v: int) : DomItem = attribute "cy" (string v)
    static member cy(v: string) : DomItem = attribute "cy" v
    static member r(v: float) : DomItem = attribute "r" (string v)
    static member r(v: int) : DomItem = attribute "r" (string v)
    static member r(v: string) : DomItem = attribute "r" v
    static member rx(v: float) : DomItem = attribute "rx" (string v)
    static member rx(v: int) : DomItem = attribute "rx" (string v)
    static member ry(v: float) : DomItem = attribute "ry" (string v)
    static member ry(v: int) : DomItem = attribute "ry" (string v)
    static member dx(v: float) : DomItem = attribute "dx" (string v)
    static member dx(v: string) : DomItem = attribute "dx" v
    static member dy(v: float) : DomItem = attribute "dy" (string v)
    static member dy(v: string) : DomItem = attribute "dy" v
    static member width(v: float) : DomItem = attribute "width" (string v)
    static member width(v: int) : DomItem = attribute "width" (string v)
    static member width(v: string) : DomItem = attribute "width" v
    static member height(v: float) : DomItem = attribute "height" (string v)
    static member height(v: int) : DomItem = attribute "height" (string v)
    static member height(v: string) : DomItem = attribute "height" v
    static member fx(v: string) : DomItem = attribute "fx" v
    static member fy(v: string) : DomItem = attribute "fy" v
    static member offset(v: string) : DomItem = attribute "offset" v
    static member pathLength(v: float) : DomItem = attribute "pathLength" (string v)
    static member refX(v: string) : DomItem = attribute "refX" v
    static member refY(v: string) : DomItem = attribute "refY" v
    static member markerWidth(v: float) : DomItem = attribute "markerWidth" (string v)
    static member markerHeight(v: float) : DomItem = attribute "markerHeight" (string v)
    static member startOffset(v: string) : DomItem = attribute "startOffset" v

    (*
        Path / geometry data
    *)
    static member d(v: string) : DomItem = attribute "d" v
    static member points(v: string) : DomItem = attribute "points" v
    static member transform(v: string) : DomItem = attribute "transform" v
    static member transformOrigin(v: string) : DomItem = attribute "transform-origin" v
    static member viewBox(v: string) : DomItem = attribute "viewBox" v
    static member preserveAspectRatio(v: string) : DomItem = attribute "preserveAspectRatio" v

    (*
        Paint
    *)
    static member fill(v: string) : DomItem = attribute "fill" v
    static member fillOpacity(v: float) : DomItem = attribute "fill-opacity" (string v)
    static member fillOpacity(v: string) : DomItem = attribute "fill-opacity" v
    static member fillRule(v: string) : DomItem = attribute "fill-rule" v
    static member stroke(v: string) : DomItem = attribute "stroke" v
    static member strokeWidth(v: float) : DomItem = attribute "stroke-width" (string v)
    static member strokeWidth(v: int) : DomItem = attribute "stroke-width" (string v)
    static member strokeWidth(v: string) : DomItem = attribute "stroke-width" v
    static member strokeLinecap(v: string) : DomItem = attribute "stroke-linecap" v
    static member strokeLinejoin(v: string) : DomItem = attribute "stroke-linejoin" v
    static member strokeDasharray(v: string) : DomItem = attribute "stroke-dasharray" v
    static member strokeDashoffset(v: string) : DomItem = attribute "stroke-dashoffset" v

    static member strokeOpacity(v: float) : DomItem = attribute "stroke-opacity" (string v)

    static member strokeMiterlimit(v: string) : DomItem = attribute "stroke-miterlimit" v
    static member opacity(v: float) : DomItem = attribute "opacity" (string v)
    static member opacity(v: string) : DomItem = attribute "opacity" v
    static member color(v: string) : DomItem = attribute "color" v
    static member stopColor(v: string) : DomItem = attribute "stop-color" v
    static member stopOpacity(v: float) : DomItem = attribute "stop-opacity" (string v)
    static member floodColor(v: string) : DomItem = attribute "flood-color" v
    static member floodOpacity(v: float) : DomItem = attribute "flood-opacity" (string v)

    (*
        Gradients / patterns / clipping
    *)
    static member gradientUnits(v: string) : DomItem = attribute "gradientUnits" v
    static member gradientTransform(v: string) : DomItem = attribute "gradientTransform" v
    static member spreadMethod(v: string) : DomItem = attribute "spreadMethod" v
    static member patternUnits(v: string) : DomItem = attribute "patternUnits" v
    static member patternContentUnits(v: string) : DomItem = attribute "patternContentUnits" v
    static member patternTransform(v: string) : DomItem = attribute "patternTransform" v
    static member clipPathUnits(v: string) : DomItem = attribute "clipPathUnits" v
    static member clipPath(v: string) : DomItem = attribute "clip-path" v
    static member clipRule(v: string) : DomItem = attribute "clip-rule" v
    static member maskUnits(v: string) : DomItem = attribute "maskUnits" v
    static member maskContentUnits(v: string) : DomItem = attribute "maskContentUnits" v
    static member mask(v: string) : DomItem = attribute "mask" v
    static member filter(v: string) : DomItem = attribute "filter" v
    static member markerStart(v: string) : DomItem = attribute "marker-start" v
    static member markerMid(v: string) : DomItem = attribute "marker-mid" v
    static member markerEnd(v: string) : DomItem = attribute "marker-end" v

    (*
        Text
    *)
    static member textAnchor(v: string) : DomItem = attribute "text-anchor" v
    static member dominantBaseline(v: string) : DomItem = attribute "dominant-baseline" v
    static member fontFamily(v: string) : DomItem = attribute "font-family" v
    static member fontSize(v: float) : DomItem = attribute "font-size" (string v)
    static member fontSize(v: string) : DomItem = attribute "font-size" v
    static member fontWeight(v: string) : DomItem = attribute "font-weight" v
    static member letterSpacing(v: string) : DomItem = attribute "letter-spacing" v

    (*
        Presentation
    *)
    static member visibility(v: string) : DomItem = attribute "visibility" v
    static member display(v: string) : DomItem = attribute "display" v
    static member pointerEvents(v: string) : DomItem = attribute "pointer-events" v
    static member cursor(v: string) : DomItem = attribute "cursor" v
    static member overflow(v: string) : DomItem = attribute "overflow" v

    (*
        References
    *)
    static member href(v: string) : DomItem = attribute "href" v
    static member xlinkHref(v: string) : DomItem = attribute "xlink:href" v

    (*
        Filter primitives
    *)
    static member in'(v: string) : DomItem = attribute "in" v
    static member in2(v: string) : DomItem = attribute "in2" v
    static member result(v: string) : DomItem = attribute "result" v
    static member stdDeviation(v: string) : DomItem = attribute "stdDeviation" v
    static member mode(v: string) : DomItem = attribute "mode" v
    static member values(v: string) : DomItem = attribute "values" v
    static member type'(v: string) : DomItem = attribute "type" v
    static member operator(v: string) : DomItem = attribute "operator" v
    static member baseFrequency(v: string) : DomItem = attribute "baseFrequency" v
    static member numOctaves(v: int) : DomItem = attribute "numOctaves" (string v)

    (*
        Animation
    *)
    static member attributeName(v: string) : DomItem = attribute "attributeName" v
    static member from'(v: string) : DomItem = attribute "from" v
    static member to'(v: string) : DomItem = attribute "to" v
    static member by(v: string) : DomItem = attribute "by" v
    static member dur(v: string) : DomItem = attribute "dur" v
    static member repeatCount(v: string) : DomItem = attribute "repeatCount" v
    static member begin'(v: string) : DomItem = attribute "begin" v
    static member end'(v: string) : DomItem = attribute "end" v
    static member calcMode(v: string) : DomItem = attribute "calcMode" v
    static member keyTimes(v: string) : DomItem = attribute "keyTimes" v
    static member keySplines(v: string) : DomItem = attribute "keySplines" v

    (*
        Generic escape hatches (incl. reactive)
    *)
    static member custom(name: string, v: string) : DomItem = attribute name v
    static member custom(name: string, c: Signal<string>) : DomItem = bindAttributeSignal name c
    static member custom(name: string, f: unit -> string) : DomItem = bindAttribute name f
