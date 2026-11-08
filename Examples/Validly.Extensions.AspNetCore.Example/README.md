# ASP.NET Core Example

## Benchmarking
I used [Oha](https://github.com/hatoo/oha) for "benchmarking".

POSTing data:
```json
{
  "username": "name",
  "password": "password",
  "email": "email@something",
  "age": 16,
  "firstName": "John",
  "lastName": "Smith"
}
```

```shell
oha -n 1000000 -c 200 -m POST -D "./request.http.json" -T "application/json" -q 70000 "http://localhost:5039/api/users"
```

**Results**

I wasn't able to find the maximum performance. Oha and other tools utilize 2x more CPU time than the ASP.NET application. I'll need at least 3 devices with this ratio to achieve 100 % CPU utilization.

With single device (MD Ryzen 7 PRO 8840HS, 1 CPU, 16 logical and 8 physical cores) I was able to get this:
- Validly: ~55 000 req/sec
- FluentValidation: ~43 000 req/sec
- No validation (just Results.Ok()): ~54 000 req/sec - A little bit less than endpoint with Validly validations, funny!

**Validly** returns this response:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": [
    {
      "detail": "Must be between {0} and {1}.",
      "resourceKey": "Validly.Validations.Between.Error",
      "args": [
        18,
        120
      ],
      "pointer": "#/age",
      "fieldName": "Age"
    },
    {
      "detail": "Must be at least {0} characters long.",
      "resourceKey": "Validly.Validations.MinLength.Error",
      "args": [
        12
      ],
      "pointer": "#/password",
      "fieldName": "Password"
    },
    {
      "detail": "Length must be between {0} and {1}.",
      "resourceKey": "Validly.Validations.LengthBetween.Error",
      "args": [
        5,
        20
      ],
      "pointer": "#/username",
      "fieldName": "Username"
    }
  ]
}
```

**FluentValidation** returns this:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "Délka pole 'Password' musí být větší nebo rovna 12 znakům. Vámi zadaná délka je 6 znaků."
    ],
    "Email": [
      "Pole 'Email' musí obsahovat platnou emailovou adresu."
    ],
    "Age": [
      "Hodnota pole 'Age' musí být mezi 18 a 99 (včetně). Vámi zadaná hodnota je 0."
    ]
  }
}
```