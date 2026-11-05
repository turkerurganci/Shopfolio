namespace Shopfolio.Infrastructure.Binding;

// The three kinds of request model (07 5.1.1; 09 5.3 item 2). A model says which kind it is by one of these markers, and the architecture rules
// (Architecture/ModelBindingRuleTests) read the markers: the name of a model ends with its kind, a page handler binds a marked model or a simple value
// only, and no model reaches an entity of 06. The notification body of the payment provider is 08's and has no marker (07 5.4.1).

#pragma warning disable CA1040 // The marker interfaces are the point: the rules find the models by them (07 5.1.1).

/// <summary>A form input model: the model of a POST handler that takes input, named <c>…Input</c> (07 5.1.1, 5.1.5).</summary>
public interface IFormInput
{
}

/// <summary>A query model: the model of a GET that takes parameters, named <c>…Query</c>; its parameters are camelCase in the query string (07 5.1.1, 5.1.5).</summary>
public interface IQueryModel
{
}

/// <summary>The request body of a machine endpoint (chunked upload), named <c>…Request</c>; its JSON names are camelCase (07 5.1.1, 5.4.2).</summary>
public interface IMachineRequest
{
}

/// <summary>The response body of a machine endpoint (chunked upload), named <c>…Response</c> (07 5.1.1, 5.4.2).</summary>
public interface IMachineResponse
{
}

#pragma warning restore CA1040
