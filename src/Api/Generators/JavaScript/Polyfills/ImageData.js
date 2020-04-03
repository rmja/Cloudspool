// https://developer.mozilla.org/en-US/docs/Web/API/ImageData/ImageData
globalThis.ImageData = class ImageData {
    constructor() {
        if (arguments[0] instanceof Uint8ClampedArray) {
            this.data = arguments[0];
            if (this.data.length % 4 !== 0) {
                throw new Error('The input data length is not a multiple of 4');
            }
            this.width = arguments[1];
            if (this.data.length % (4 * this.width) !== 0) {
                throw new Error('The input data length is not a multiple of (4 * width)');
            }
            this.height = arguments[2] || this.data.length / (4 * this.width);
        }
        else {
            this.width = arguments[0];
            this.height = arguments[1];
            this.data = new Uint8ClampedArray(this.width * this.height * 4);
        }
    }
}