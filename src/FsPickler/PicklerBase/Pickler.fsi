﻿namespace Nessos.FsPickler

    open System
    open System.Collections.Generic
    open System.Runtime.Serialization

    open Nessos.FsPickler.TypeCache

    [<AbstractClass>]
    type Pickler =
        class
            internal new : unit -> Pickler

            /// Type of values serialized by this pickler.
            abstract member Type : System.Type

            /// The underlying type that this pickler implements.
            abstract member ImplementationType : System.Type

            /// Cast to a pickler of specific type.
            abstract member Cast : unit -> Pickler<'S>

            /// Specifies if pickled objects are to be cached by reference.
            abstract member IsCacheByRef : bool

            /// Specifies if instances of this pickler type are of fixed size.
            abstract member IsOfFixedSize : bool

            /// Specifies if instances of this pickler type can be cyclic objects.
            abstract member IsRecursiveType : bool

            /// Pickler generation metadata.
            abstract member PicklerInfo : PicklerInfo
            
            /// Specifies if this pickler can be applied to proper subtypes.
            abstract member UseWithSubtypes : bool

            abstract member internal UntypedRead : state:ReadState -> tag:string -> obj
            abstract member internal UntypedWrite : state:WriteState -> tag:string -> value:obj -> unit

            abstract member internal TypeKind : TypeKind
            abstract member internal Unpack : IPicklerUnpacker<'U> -> 'U
            abstract member internal InitializeFrom : other:Pickler -> unit
        end

    and [<AbstractClass>] Pickler<'T> =
        class
            inherit Pickler
            internal new : unit -> Pickler<'T>
            abstract member Read : state:ReadState -> tag:string -> 'T
            abstract member Write : state:WriteState -> tag:string -> value:'T -> unit

            override Type : Type
            override Unpack : IPicklerUnpacker<'R> -> 'R
        end

    and internal IPicklerUnpacker<'U> =
        interface
            abstract member Apply : Pickler<'T> -> 'U
        end

    and IObjectVisitor =
        interface
            abstract member Visit<'T> : 'T -> unit
        end

    and IPicklerResolver =
        interface
            /// Identifies if instances of given type can be serialized.
            abstract member IsSerializable : System.Type -> bool
            /// Identifies if instances of given type can be serialized.
            abstract member IsSerializable<'T> : unit -> bool

            /// Attempt to generate a pickler instance for given type.
            abstract member Resolve : System.Type -> Pickler
            /// Attempt to generate a pickler instance for given type.
            abstract member Resolve : unit -> Pickler<'T>
        end

    and WriteState =
        class
            internal new : 
                formatter:IPickleFormatWriter * resolver:IPicklerResolver * reflectionCache:ReflectionCache * 
                    ?streamingContext:StreamingContext * ?visitor : IObjectVisitor -> WriteState

            member internal ResetCounters : unit -> unit
            member internal CyclicObjectSet : HashSet<int64>
            member internal ObjectStack : Stack<int64>
            member internal Formatter : IPickleFormatWriter
            member internal NextObjectIsSubtype : bool
            member internal ObjectIdGenerator : ObjectIDGenerator
            member internal PicklerResolver : IPicklerResolver
            member internal ReflectionCache : TypeCache.ReflectionCache
            member StreamingContext : StreamingContext
            member internal Visitor : IObjectVisitor option
            member internal TypePickler : Pickler<System.Type>
            member internal NextObjectIsSubtype : bool with set
        end

    and ReadState =
        class
            internal new : 
                formatter:IPickleFormatReader * resolver:IPicklerResolver * reflectionCache:ReflectionCache * 
                    ?streamingContext:StreamingContext * ?visitor : IObjectVisitor -> ReadState

            member internal GetObjectId : isArray:bool -> int64
            member internal RegisterUninitializedArray : array:System.Array -> unit
            member internal ResetCounters : unit -> unit
            member internal FixupIndex : Dictionary<int64,(System.Type * obj)>
            member internal Formatter : IPickleFormatReader
            member internal NextObjectIsSubtype : bool
            member internal ObjectCache : Dictionary<int64,obj>
            member internal PicklerResolver : IPicklerResolver
            member internal ReflectionCache : TypeCache.ReflectionCache
            member StreamingContext : StreamingContext
            member internal Visitor : IObjectVisitor option
            member internal TypePickler : Pickler<System.Type>
            member internal NextObjectIsSubtype : bool with set
        end