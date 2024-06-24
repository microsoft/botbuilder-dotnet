## AdaptiveExpressions on System.Text.Json overview

To make AdaptiveExpressions library work with [AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Cwindows), it was necessary to migrate from Newtonsoft to System.Text.Json. This has user-visible API and implementation differences.

### Expanded IMemory interface and constrained expression return types

The expression system calls out to the IMemory object to get and set values into a user-provided object graph. It is expected that TryGetValue returns only primitive types like number, string. But TryGetValue can also return lists or other objects. In many cases those objects are opaque and are just passed back into IMemory.SetValue or returned back to the caller, but in other cases the expression engine used to try to manipulate the objects or serialize them to json. Because all such methods would require reflection, those responsibilities are now delegated back to IMemory and the  expression engine only understands primitive types, a small number of List types (`IList` and `List<object>`), and System.Text.Json types (e.g. `JsonArray` and `JsonObject`).

In some cases the expression engine will create a list (e.g. if you foreach over an object), and in this case the expression will create a `List<object>` and the user-implemented IMemory is expected to handle this externally-created object as well.

### Added JsonNodeMemory

For callers that don't want to implement IMemory, the easiest way to migrate to AOT is to use JsonObjects to store the data you want to evaluate expressions against and wrap them in JsonNodeMemory to pass in to evaluation functions. You can also use JsonSerializer.SerializeToNode to serialize an existing non-json object into JsonNode in an AOT-compatible way.

### Many methods now take JsonTypeInfo

System.Text.Json supports AOT by not relying on reflection, and instead the link between a type and its converter is via `JsonTypeInfo`. You get one of these using the [STJ source generator](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0).

Implementation within and APIs on AdaptiveExpressions that would have needed to do json serialization on unknown types now are marked as `[RequiresDynamicCode]` and `[RequiresUnreferencedCode]` and there are overloads of those methods and types that take JsonTypeInfo which can be used instead.

### Testing AOT mode

It's difficult to publish & test component in AOT compilation mode, so for testing components in AOT mode we turn on AOT warnings and ensure that the tests and product code are AOT-warning free to convince ourselves that the component will behave correctly when compiled as AOT with trimming on.

AdaptiveExpressionsSTJ.Tests are using some of the AOT patterns but still test SimpleObjectMemory paths and other non-AOT compatible paths. AdaptiveExpressionsSTJ.AOT.Tests is a copy of most of the tests from AdaptiveExpressionsSTJ.Tests but rewritten to be going through exclusively AOT-compatible modes. Functionally this means only testing against the JsonNodeMemory backing implementation and thus some of the round-trip tests aren't particularly interesting but they remain to show others how to convert such code in the future.