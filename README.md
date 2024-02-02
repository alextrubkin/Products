# Product API
This api has a simple responsibility. There are nt much endpoints and business logic. That is why I decided to use Minimal APIs.
There are also no need at entity tracking and any other object modifications. That's why I decided to use Dapper ORM 

### Local development
Navigate to the Run the following command to generate the Bearer token locally:

``` cmd
cd ./Products.Api
dotnet user-jwts create --scope "products_api" --role "reader"
```
This will generate a token that can be used to authenticate with the Products API. The token will be printed to the console.

## Endpoints
### GET /products
This endpoint returns a list of products. It requires a valid Bearer token to be included in the `Authorization` header.
It also allows to filter the products by colour. The colour is passed as a query string parameter.
Pagination is also supported. The page number and page size are passed as query string parameters.

#### Request
``` http
GET /products
Authorization: Bearer <token>
```

#### Response
``` http
HTTP/1.1 200 OK
Content-Type: application/json
[
    {
        "id": 1,
        "name": "Product 1",
        "description": "This is the first product",
        "price": 9.99,
        "colour": "Red"
    },
    {
        "id": 2,
        "name": "Product 2",
        "description": "This is the second product",
        "price": 19.99,
        "colour": "White"
    }
]
```
