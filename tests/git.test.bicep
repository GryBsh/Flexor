targetScope = 'local'
extension flexor with {
  flexorPath: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}


resource clone 'Flexor/repo@2026-01-01' = {
  source: 'https://github.com/GryBsh/Flexor.git'
  path: 'output/Flexor'
}


resource pull 'Flexor/repo@2026-01-01' existing = {
  path: clone.path
}
/*
resource regPull 'Flexor/repo@2026-01-01' = {
  path: clone.path
}
*/
output clonePath string = clone.path
output pullPath string = pull.path
//output regPulledPath string = regPull.path
