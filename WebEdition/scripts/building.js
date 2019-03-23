var Building = function(left, top, width, height, color, label) {
    this.left = left;
    this.top = top;
    this.width = width;
    this.height = height;
    this.color = color;
    this.label = label;
    this.enableLabel = false;
    this.borderless = false;
    this.road = false;
};

Building.FromObject = function (obj) {
    // type conversions needed to support creating building directly from data supplied by the webservice
    var b = new Building(
        Convert.int(obj.left),
        Convert.int(obj.top),
        Convert.int(obj.width),
        Convert.int(obj.height),
        obj.color,
        obj.label
    );
    b.enableLabel = Convert.bool(obj.enableLabel);
    b.borderless = Convert.bool(obj.borderless);
    b.road = Convert.bool(obj.road);
    return b;
};

Building.prototype.Position = function (point) {
    if (point) {
        this.left = point.x;
        this.top = point.y;
        return this;
    } else {
        return new Point(this.left, this.top);
    }
};

Building.prototype.Size = function () {
    return new Size(this.width, this.height);
};

Building.prototype.Rect = function () {
    return new Rect(this.left, this.top, this.width, this.height);
};

Building.prototype.IsValid = function () {
    return !(this.width < 1 || this.height < 1 || this.color.length != 7);
};

Building.prototype.FlipSize = function() {
    var tmp = this.width;
    this.width = this.height;
    this.height = tmp;
};