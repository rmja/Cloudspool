module.exports = {  
  entry: {
    app: './app/src/app.ts'
  },
  output: {
    path: 'wwwroot',
    filename: 'app.js'
    //filename: 'app.[hash].js'
  },
  resolve: {
    extensions: ['', '.ts', '.js', '.html']
  },
  module: {
    loaders: [
      { test: /\.ts$/, loader: 'ts-loader' },
      { test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)$/, loader: 'file' },
      { test: /\.html$/, loader: 'raw' }
    ]
  }
}