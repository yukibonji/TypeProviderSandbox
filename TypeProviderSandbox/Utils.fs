﻿[<AutoOpen>]
module internal TypeProviderSandbox.Utils

open System.Runtime.Caching
open System
open ProviderImplementation.ProvidedTypes
open System.Reflection
 
type MemoryCache with  
    member x.GetOrAdd key (value: Lazy<_>) = 
        let policy = CacheItemPolicy()
        policy.SlidingExpiration <- TimeSpan.FromHours 24.
        match x.AddOrGetExisting(key, value, policy) with
        | :? Lazy<ProvidedTypeDefinition> as item -> item.Value 
        | x -> 
            assert(x = null)
            value.Value

let private makeMethod<'a> isStatic name parameters = 
    ProvidedMethod(name, parameters, typeof<'a>, IsStaticMethod = isStatic)

let staticMethod<'a> = makeMethod<'a> true
let instanceMethod<'a> = makeMethod<'a> false
let staticParam<'a> name = ProvidedStaticParameter(name, typeof<'a>)

let private makeProperty<'a> isStatic name getter =
    ProvidedProperty (name, typeof<'a>, IsStatic = isStatic, GetterCode = getter)
let staticProperty<'a> = makeProperty<'a> true
let property<'a> = makeProperty<'a> false

let withStaticParams staticParams (m: ProvidedMethod) f = 
     m.DefineStaticParameters (staticParams, f); m

let addTo (ty: ProvidedTypeDefinition) (m: MemberInfo) = ty.AddMember m
let withCode code (m: ProvidedMethod) = m.InvokeCode <- code; m
