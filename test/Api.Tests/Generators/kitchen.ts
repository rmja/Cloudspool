interface Model {
    isFood: boolean;
    case: {
        id: number;
        packageId: number;
        itemId: number;
        createdByEmployeeName?: string
        note: string;
        arrived: string /* Date */;
        departed?: string /* Date */;
    },
    itemName: string;
    sequenceNumber: number;
    printedByEmployeeName?: string;
    printed: string /* Date */;
    isCancellation: boolean;
    added?: Line[];
    changed?: { baseline: Line, target: Line }[];
    removed?: Line[];
}

interface Line {
    name: string;
    quantity?: number;
    note: string;
}

export default class Builder {
    writer: StarLineWriter;
    contentType = "application/starline";

    build(model: Model) {
        let buffer = new WriteBuffer();
        this.writer = new StarLineWriter(buffer);

        this.writer.writeInitialize();
        this.writer.writeSetCodeTable(CodeTable.Latin1_Windows1252);

        this.writeHeader(model);
        if (model.isCancellation) {
            this.writeCancel(model);
        }
        else {
            this.writeDiff(model);
        }
        this.writeFooter(model);

        for (let i = 0; i < 3; i++) {
            this.writer.writeNewline();
        }
        this.writer.cutPaper(false);

        return buffer.toArray();
    }

    private writeHeader(model: Model) {
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeSetHorizontalTabPositions([11]);

        if (model.case.createdByEmployeeName) {
            this.writer.writeString("Startet af");
            this.writer.writeHorizontalTab();
            this.writer.writeString(model.case.createdByEmployeeName);
            this.writer.writeNewline();
        }

        if (model.printedByEmployeeName) {
            this.writer.writeString("Udskr. af");
            this.writer.writeHorizontalTab();
            this.writer.writeString(model.printedByEmployeeName);
            this.writer.writeNewline();
        }

        this.writer.writeString("Bon nr.");
        this.writer.writeHorizontalTab();
        this.writer.writeString(`${model.sequenceNumber}`);
        this.writer.writeNewline();

        let arrived = new Date(model.case.arrived);
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeString("Startet");
        this.writer.writeHorizontalTab();
        this.writer.writeString(formatTime(arrived));
        this.writer.writeSetFontSize(0, 0);
        this.writer.writeString(" " + formatDate(arrived));
        this.writer.writeNewline();

        let printed = new Date(model.printed);
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeString("Udskrevet");
        this.writer.writeHorizontalTab();
        this.writer.writeString(formatTime(printed));
        this.writer.writeSetFontSize(0, 0);
        this.writer.writeString(" " + formatDate(printed));
        this.writer.writeNewline();

        this.writer.writeSetFontSize(2, 2);
        this.writer.writeString(model.itemName.substr(0, 16));
        this.writer.writeNewline();

        this.writeHorizontalLine();
    }

    private writeDiff(model: Model) {
        let isFirst = model.sequenceNumber == 1

        this.writer.writeSetHorizontalTabPositions([7]);
        if (model.added && model.added.length) {
            if (!isFirst) {
                this.writeMergeBlockHeader("TILFØJET");
            }
            for (const line of model.added) {
                if (line.quantity) {
                    if (isFirst) {
                        this.writeQuantity(line.quantity, false);
                    }
                    else {
                        this.writeDifference(line.quantity);
                    }
                    this.writer.writeHorizontalTab();
                    this.writeLineName(line.name);
                    this.writer.writeNewline();
                }
                else {
                    this.writeLineName(line.name);
                    this.writer.writeNewline();
                }
                if (line.note) {
                    this.writeLineNote(line.note);
                }
            }
        }
        if (model.changed && model.changed.length) {
            this.writeMergeBlockHeader("ÆNDRET");
            for (const diff of model.changed) {
                let baseline = diff.baseline
                let target = diff.target
                if (target.quantity) {
                    this.writeQuantity(baseline.quantity, true);
                    this.writeDifference(target.quantity - baseline.quantity);
                    this.writer.writeSetFontSize(0, 0);
                    this.writer.writeString("=");
                    this.writeQuantity(target.quantity, false);
                    this.writer.writeHorizontalTab();
                    this.writeLineName(target.name);
                    this.writer.writeNewline();
                }
                else {
                    this.writeLineName(target.name);
                    this.writer.writeNewline();
                }
                if (target.note) {
                    this.writeLineNote(target.note);
                }
                else if (baseline.note) {
                    this.writer.writeString("Notat slettet");
                }
            }
        }
        if (model.removed && model.removed.length) {
            this.writeMergeBlockHeader("ANNULERET");
            for (const line of model.removed) {
                if (line.quantity) {
                    this.writeDifference(-1 * line.quantity);
                    this.writer.writeHorizontalTab();
                    this.writeLineName(line.name);
                    this.writer.writeNewline();
                }
                else {
                    this.writeLineName(line.name);
                    this.writer.writeNewline();
                }
                if (line.note) {
                    this.writeLineNote(line.note);
                }
            }
        }
        this.writeHorizontalLine();
    }

    private writeCancel(model: Model) {
        this.writer.writeSetFontSize(2, 2);
        this.writer.writeString("FULDT ANNULERET!");
        this.writer.writeNewline();
        this.writeHorizontalLine();
    }

    private writeFooter(model: Model) {
        if (model.case.note) {
            let note = model.case.note.trim();
            if (note) {
                this.writer.writeSetFontSize(1, 1);
                this.writer.writeNewline();
                this.writer.writeString(note);
                this.writer.writeNewline();
            }
        }
    }

    private writeMergeBlockHeader(header: string) {
        this.writer.writeSetFontSize(2, 2);
        this.writer.writeString(header);
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeNewline();
    }

    private writeQuantity(quantity: number, tiny: boolean) {
        if (tiny) {
            this.writer.writeSetFontSize(0, 0);
        }
        else {
            this.writer.writeSetFontSize(1, 1);
        }
        if (Math.round(quantity) === quantity) {
            this.writer.writeString(quantity.toString())
        }
        else {
            this.writer.writeString(quantity.toFixed(1))
        }
    }

    private writeDifference(difference: number) {
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeString(difference < 0 ? "-" : "+");
        if (Math.round(difference) === difference) {
            this.writer.writeString(Math.abs(difference).toString());
        }
        else {
            this.writer.writeString(Math.abs(difference).toFixed(1));
        }
    }

    private writeLineName(name: string) {
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeString(name.substr(0, this.writer.LINEWIDTH / 2 - 7));
    }

    private writeLineNote(note: string) {
        this.writer.writeSetFontSize(1, 1);
        for (let line of note.split(",")) {
            line = line.trim();
            if (line.length) {
                this.writer.writeHorizontalTab();
                if (line[0] === "-") {
                    this.writer.writeString("÷" + line.substr(1, 16));
                }
                else if (line[0] === "+") {
                    this.writer.writeString("+" + line.substr(1, 16));
                }
                else {
                    this.writer.writeString(" " + line.substr(0, 16));
                }
                this.writer.writeNewline();
            }
        }
    }

    private writeHorizontalLine() {
        this.writer.writeSetFontSize(1, 1);
        this.writer.writeString("—".repeat(this.writer.LINEWIDTH / 2));
        this.writer.writeNewline();
    }
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
}

class WriteBuffer {
    private buffer = new ArrayBuffer(64);
    public written = 0;

    write(values: number[]) {
        this.ensure(values.length);
        (new Uint8Array(this.buffer)).set(new Uint8Array(values), this.written);
        this.written += values.length;
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