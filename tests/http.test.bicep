targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}

var body = {
  Works: true
}

// HTTP
resource get 'Flexor/http@2026-01-01' existing = {
  name: 'get'
  url: 'https://postman-echo.com/get'
  enableLogging: false
}

resource post 'Flexor/http@2026-01-01' = {
  name: 'post'
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
  dependsOn: [get] 
}

output getStatusCode int = get.statusCode
output getResponseBody string = get.output
output postStatusCode int = post.statusCode
output postResponseBody string = post.output
