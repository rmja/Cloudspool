interface Model {
    vendorDataResource: string;
    slipId: number;
    case: {
        id: number;
        packageId: number;
        itemId: number;
        createdByEmployeeName?: string
        note: string;
        arrived: string /* Date */;
        departed?: string /* Date */;
    },
    lines: Line[],
    itemName: string;
    printedByEmployeeName: string;
    grossTotal: number;
    discountTotal: number;
    netTotal: number;
    netTaxTotal: number;
    netEuroTotal: number;
    printed: string /* Date */;
}

interface Line {
    name: string;
    quantity: number;
    unitPrice: number;
    discount: number;
}

interface VendorData {
    logoResource: string;
    address: string;
    postalCode: number;
    city: string;
    phone: number;
    cvr: number;
    email: string;
    logoOffsetLeft?: number;
}

declare function require(path: string): any;

export default class Builder {
    writer: StarLineWriter;
    contentType = "application/starline";

    build(model: Model) {
        let vendorData: VendorData = require(`resources/${model.vendorDataResource}.json`);

        let buffer = new WriteBuffer();
        this.writer = new StarLineWriter(buffer);

        this.writer.writeInitialize();
        this.writer.writeSetCodeTable(CodeTable.Latin1_Windows1252);

        this.header(vendorData);
        // this.caseLinesHeader();
        // this.caseLines(model);
        // this.caseLinesFooter(model);
        // this.footer(model, vendorData);

        for (let i = 0; i < 5; i++) {
            this.writer.writeNewline();
        }
        this.writer.cutPaper(false);

        return buffer.toArray();
    }

    private header(vendorData: VendorData) {
        if (vendorData.logoResource) {
            let imageData: ImageData = require(`resources/${vendorData.logoResource}.bmp`);

            if (imageData) {
                let offsetLeft = vendorData.logoOffsetLeft ?? 0;
                this.writer.writeImage(imageData, offsetLeft);
            }
        }

        let postalCode = vendorData.postalCode.toString();
        let length = vendorData.address.length + 1 + 1 + postalCode.length + 1 + vendorData.city.length;
        let spacings = (length % 2) ? 1 : 2;
        let indentation = (this.writer.LINEWIDTH - (length + spacings)) / 2;

        this.writer.writeNewline();
        this.writer.writeNewline();
        this.writer.writeString(" ".repeat(indentation) + vendorData.address + " " + "·".repeat(spacings) + " " + postalCode + " " + vendorData.city)
        this.writer.writeNewline();
        this.writer.writeNewline();
    }

    // private caseLinesHeader() {
    //     this.writer.writeSetHorizontalTabPositions([4, this.writer.LINEWIDTH - 9 - 1 - 9, this.writer.LINEWIDTH - 9]);
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString("       á");
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString("    Pris");
    //     this.writer.writeNewline();
    // }

    // private caseLines(model: Model) {
    //     const padPrice = (price: number) => (" ".repeat(4) + price.toFixed(2)).slice(-8); // 12345.67
    //     for (const line of model.lines) {
    //         this.quantity(line.quantity);
    //         this.writer.writeHorizontalTab();
    //         this.writer.writeString(line.name.substr(this.writer.LINEWIDTH - 4 - 1 - 9 - 1 - 9));
    //         this.writer.writeHorizontalTab();
    //         this.writer.writeString(padPrice(line.unitPrice));
    //         this.writer.writeHorizontalTab();
    //         this.writer.writeString(padPrice(line.quantity * line.unitPrice));
    //         this.writer.writeNewline();

    //         if (line.discount) {
    //             this.writer.writeHorizontalTab();
    //             this.writer.writeString("Rabat");
    //             this.writer.writeHorizontalTab();
    //             this.writer.writeHorizontalTab();
    //             this.writer.writeString(padPrice(-line.discount));
    //             this.writer.writeNewline();
    //         }
    //     }
    // }

    // private caseLinesFooter(model: Model) {
    //     const padTotalPrice = (price: number) => (" ".repeat(5) + price.toFixed(2)).slice(-6); // 123456.78

    //     this.writer.writeString(" ".repeat(this.writer.LINEWIDTH - 9) + "·········");
    //     this.writer.writeNewline();
    //     this.writer.writeSetHorizontalTabPositions([11, this.writer.LINEWIDTH - 10]);

    //     if (model.discountTotal) {
    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString('Total:')
    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString(padTotalPrice(model.grossTotal));
    //         this.writer.writeNewline();

    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString('Rabat:');
    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString(padTotalPrice(-model.discountTotal));
    //         this.writer.writeNewline();
    //     }

    //     this.writer.writeSetEmphasize(true);
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('At betale:');
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString(padTotalPrice(model.netTotal));
    //     this.writer.writeSetEmphasize(false);
    //     this.writer.writeNewline();

    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('Heraf 25% moms:');
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString(padTotalPrice(model.netTaxTotal));
    //     this.writer.writeNewline();

    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('At betale €:');
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString(padTotalPrice(model.netEuroTotal));
    //     this.writer.writeNewline();
    //     this.writer.writeNewline();
    //     this.writer.writeNewline();
    // }
    // private footer(model: Model, vendorData: VendorData) {
    //     let maxLength = "xx/xx-xxxx xx:xx".length;
    //     this.writer.writeSetHorizontalTabPositions([4, this.writer.LINEWIDTH - 4 - maxLength]);
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('Bon nr.');
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString(model.slipId.toString());
    //     this.writer.writeNewline();

    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('Sag nr.');
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString(model.case.id.toString());
    //     this.writer.writeNewline();

    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('Emne');
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString(model.itemName.substr(maxLength));
    //     this.writer.writeNewline();

    //     if (model.printedByEmployeeName) {
    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString('Udskrevet af');
    //         this.writer.writeHorizontalTab()
    //         this.writer.writeString(model.printedByEmployeeName);
    //         this.writer.writeNewline();
    //     }

    //     let arrived = new Date(model.case.arrived);
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString('Startet');
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString(formatDate(arrived) + " " + formatTime(arrived));
    //     this.writer.writeNewline();

    //     let printed = new Date(model.printed);
    //     this.writer.writeHorizontalTab()
    //     this.writer.writeString('Udskrevet');
    //     this.writer.writeHorizontalTab();
    //     this.writer.writeString(formatDate(printed) + " " + formatTime(printed));
    //     this.writer.writeNewline();
    //     this.writer.writeNewline();

    //     for (const line of [`Tlf: +45 ${vendorData.phone} · CVR nr: ${vendorData.cvr}`, vendorData.email]) {
    //         this.writer.writeString(' '.repeat((this.writer.LINEWIDTH - line.length) / 2) + line);
    //         this.writer.writeNewline();
    //     }
    // }

    // private quantity(quantity: number) {
    //     if (Math.round(quantity) === quantity) {
    //         this.writer.writeString(quantity.toString());
    //     }
    //     else {
    //         this.writer.writeString(quantity.toFixed(1));
    //     }
    // }
}

const ESC = 0x1B
const GS = 0x1D
const pad2 = (input: number) => ("0" + input).slice(-2);
const formatTime = (date: Date) => pad2(date.getHours()) + ":" + pad2(date.getMinutes());
const formatDate = (date: Date) => pad2(date.getDate()) + "/" + pad2(date.getMonth() + 1) + "-" + date.getFullYear();

enum CodeTable {
    Latin1_Windows1252 = 32,
    Nordic_PC865 = 9,
    Euro_PC858 = 4
}

class StarLineWriter {
    LINEWIDTH: number;
    private buffer: WriteBuffer;
    private encodingBuffer: number[];

    constructor(buffer: WriteBuffer) {
        this.LINEWIDTH = 48;
        this.buffer = buffer;
    }

    writeInitialize() {
        this.buffer.write([ESC, "@".charCodeAt(0)]);
    }

    writeSetCodeTable(value: CodeTable) {
        this.buffer.write([ESC, GS, "t".charCodeAt(0), value]);

        let extensionChars: string;
        switch (value) {
            case CodeTable.Latin1_Windows1252:
                // https://github.com/ashtuchkin/iconv-lite/blob/master/encodings/sbcs-data-generated.js
                extensionChars = "€�‚ƒ„…†‡ˆ‰Š‹Œ�Ž��‘’“”•–—˜™š›œ�žŸ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
                break;
            default: throw new Error("Not supported");
        }

        let asciiChars = "";
        for (var i = 0; i < 128; i++) {
            asciiChars += String.fromCharCode(i);
        }
        let allChars = asciiChars + extensionChars;
        this.encodingBuffer = [];
        for (let i = 0; i < allChars.length; i++) {
            this.encodingBuffer[allChars.charCodeAt(i)] = i;
        }
    }

    writeSetHorizontalTabPositions(positions: number[]) {
        this.buffer.write([ESC, "D".charCodeAt(0), ...positions, 0]);
    }

    writeSetFontSize(width: number, height: number) {
        this.buffer.write([ESC, "i".charCodeAt(0), width, height]);
    }

    writeSetEmphasize(enabled: boolean) {
        this.buffer.write([ESC, enabled ? "E".charCodeAt(0) : "F".charCodeAt(0)]);
    }

    writeNewline() {
        this.buffer.write([0x0A]);
    }

    writeString(value: string) {
        let encoded = [];
        for (let i = 0; i < value.length; i++) {
            let char = this.encodingBuffer[value.charCodeAt(i)];

            if (char === undefined) {
                char = '?'.charCodeAt(0);
            }
            encoded.push(char);
        }
        this.buffer.write(encoded);
    }

    writeHorizontalTab() {
        this.buffer.write([0x09]);
    }

    cutPaper(partial: boolean) {
        this.buffer.write([ESC, "d".charCodeAt(0), partial ? 1 : 0])
    }

    writeImage(bitmap: ImageData, offsetLeft: number) {
        function isBlack(r: number, g: number, b: number) {
            // http://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
            // http://www.w3.org/TR/AERT#color-contrast
            let luminance = r * 0.299 + g * 0.587 + b * 0.114;
            return luminance <= 127;
        }

        let totalWidth = offsetLeft + bitmap.width;
        let yLastOffset = Math.floor(bitmap.height / 24) * 24;
        let marshalled = new Uint8Array(new ArrayBuffer(Math.ceil(totalWidth / 8) * 24)); // 1 bit per pixel in a 24 bit tall slice
        // Set multiple slices, each with 24 pixels in the height
        for (let yOffset = 0; yOffset < bitmap.height; yOffset += 24) {
            // Define the full slice as white
            marshalled.fill(0);
            let index = 0;
            let yEnd = Math.max(yOffset + 24, bitmap.height);
            // Marshal one row at a time (up to 24 rows)
            for (let y = yOffset; y < yEnd; y++) {
                for (let xOffset = 0; xOffset < totalWidth; xOffset += 8) {
                    let byte = 0
                    let xEnd = Math.max(xOffset + 8, totalWidth);
                    for (let x = xOffset; x < xEnd; x++) {
                        if (x < offsetLeft) {
                            // Not in image data yet
                            continue;
                        }

                        let start = (y * bitmap.width + x - offsetLeft) * 4;
                        let r = bitmap.data[start + 0];
                        let g = bitmap.data[start + 1];
                        let b = bitmap.data[start + 2];
                        if (isBlack(r, g, b)) {
                            byte |= 0x80 >>> (x & 7);
                        }
                    }
                    marshalled[index++] = byte;
                }
            }

            let n = marshalled.byteLength / 24;
            this.buffer.write([ESC, "k".charCodeAt(0), n & 0xff, n >> 8]);
            this.buffer.writeArray(marshalled);

            if (yOffset < yLastOffset) {
                // All but last - move 24 pixels down.
                this.buffer.write([ESC, 'I'.charCodeAt(0), 24]);
            }
        }
    }
}

class WriteBuffer {
    private buffer = new ArrayBuffer(64);
    public written = 0;

    write(values: number[]) {
        this.writeArray(new Uint8Array(values));
    }

    writeArray(array: Uint8Array) {
        this.ensure(array.byteLength);
        (new Uint8Array(this.buffer)).set(array, this.written);
        this.written += array.byteLength;
    }

    toArray() {
        return new Uint8Array(this.buffer, 0, this.written);
    }

    private ensure(amount: number) {
        let needed = this.written + amount;
        if (needed > this.buffer.byteLength) {
            let newSize = Math.floor((needed + 64 - 1) / 64) * 64;
            let newBuffer = new ArrayBuffer(newSize);
            let array = new Uint8Array(newBuffer);
            array.set(new Uint8Array(this.buffer, 0, this.written));
            this.buffer = newBuffer;
        }
    }
}