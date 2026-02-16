targetScope = 'local'
extension flexor with {
  flexorPath : '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

var body = {
  Works: true
}

// HTTP
resource get 'Flexor/http@2026-01-01' existing = {
  url: 'https://postman-echo.com/get'
}

resource post 'Flexor/http@2026-01-01' = {
  url: 'https://postman-echo.com/post'
  method: 'POST'
  contentType: 'application/json'
  headers: [
    {
      name: 'accept'
      value: 'application/json'
    }
  ]
  body: string(body)
  expectedStatusCodes: [200]
  dependsOn: [get] 
}

output getStatusCode int = get.statusCode
output getResponseBody string = get.output
output postStatusCode int = post.statusCode
output postResponseBody string = post.output

output postWorks bool = json(post.output).json.Works
