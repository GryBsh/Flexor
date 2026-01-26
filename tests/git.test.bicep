targetScope = 'local'
extension flexor with {
  pathRoot: '../.bicep/flexor'
  logOptions: {
    disableRollover: true
  }
}


resource clone 'Flexor/repo@2026-01-01' = {
  name: 'CloneRepo'
  source: 'https://github.com/GryBsh/Flexor.git'
  path: 'output/Flexor'
}

/*
resource pull 'Flexor/repo@2026-01-01' existing = {
  name: 'PullRepo'
  path: clone.path
}

resource regPull 'Flexor/repo@2026-01-01' = {
  name: 'PullAgain'
  path: clone.path
}
*/
output clonePath string = clone.path
//output pulledPath string = pull.path
//output regPulledPath string = regPull.path
