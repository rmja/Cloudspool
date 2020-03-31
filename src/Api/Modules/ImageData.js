// https://developer.mozilla.org/en-US/docs/Web/API/ImageData/ImageData
export default class ImageData {
    constructor() {
        if (arguments[0] instanceof Uint8ClampedArray) {
            this.data = arguments[0];
            this.width = arguments[1];
            this.height = arguments[2] || this.data.length / (4 * this.width);
        }
        else {
            this.width = arguments[0];
            this.height = arguments[1];
            this.data = new Uint8ClampedArray(this.width * this.height * 4);
        }
    }
}