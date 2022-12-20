using UnionTypes;

IntOrString value = "zomg";
var result = Result<int>.Success(500);
var optional = Option<int>.Some(100);
