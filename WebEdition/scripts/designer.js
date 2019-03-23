//** Editor mouse behavior
// + MouseMove
//  - no action active: highlight object under mouse
//   -   or start dragging (selection rect or move selected objects)
//  - placing an object: move object to mouse
// + MouseClick (left)
//   - no action active: select objects (shift/ctrl mechanics)
//   -   or prepare dragging
//   - placing an object: place object if it fits (collision detection)
// + MouseDblClick (left)
//   - get properties of clicked objects
// + MouseClick (right)
//   - no action active: remove object at mouse
//   - placing an object: stop placing objects
// + MouseClick (middle)
//   - placing an object: flip object dimensions
// + MouseWheel
//   - change zoom level (grid cell size)
// + Hotkeys:
//   - entf: remove selected objects, if any

/**
 * Anno Designer - Web Edition
 * Date: 09.01.2013
 * @version 1.0
 * @author Jan Christoph Bernack
 * @requires jQuery, jQuery-ui, jQuery-miniColors, json2
 * @param {Object} options Option set to overwrite any defaults.
 * @constructor
 */
var Designer = function (options) {
    // extend defaults with given options
    this._options = $.extend(true, {}, Designer.defaultOptions, options);
    // get reference to container element
    var container = $("#" + this._options.containerId);
    this._container = container;
    // prepare container, canvas and buttonpane
    container.html("");
    this._resizer = $(document.createElement("div")).addClass("resizer");
    this._canvas = document.createElement("canvas");
    this._canvas.tabIndex = 0;
    this._resizer.append(this._canvas);
    container.append(this._resizer);
    this._ctx = this._canvas.getContext("2d");
    this._registerEvents();
    if (this._options.enableEditing) {
        this._createButtonpane();
    }
    // render an empty layout
    this.SetSize();
    this.Render();
    this.RefreshResizer();
};

/**
 * Default options used for every new instance of the designer.
 * @type {Object}
 */
Designer.defaultOptions = {
    serviceUrl: "rest/",
    containerId: "editor",
    layoutReset: $.noop,
    layoutChanged: $.noop,
    layoutDeleted: $.noop,
    autoSize: true,
    enableEditing: true,
    drawGrid: true,
    grid: 15,
    width: 15,
    height: 10,
    zoomSpeed: 1.1,
    spacing: 0.5 //TODO: implement normalization and set to 1
};

/**
 * Enumeration of mouse interaction states.
 * Values are arbitrary but need to be unique.
 * @type {Object}
 */
Designer.State = {
    // used if not dragging
    Standard: 0,
    // used to drag the selection rect
    SelectionRectStart: 1,
    SelectionRect: 2,
    // used to drag objects around
    DragSelectionStart: 3,
    DragSingleStart: 4,
    DragSelection: 5,
    DragAllStart: 6,
    DragAll: 7
};

/**
 * Holds an array of all objects on the current layout.
 * @type {Array}
 * @private
 */
Designer.prototype._objects = [];
/**
 * Holds an array of all objects which are highlighted.
 * @type {Array}
 * @private
 */
Designer.prototype._selectedObjects = [];
/**
 * The object which is currently being placed, or null if none.
 * @type {Building}
 * @private
 */
Designer.prototype._currentObject = null;
/**
 * The object currently under the mouse.
 * @type {Building}
 * @private
 */
Designer.prototype._hoveredObject = null;

/**
 * Holds the layout object received from the server
 * @type {Object}
 * @private
 */
Designer.prototype._layout = null;
/**
 * Current interaction state defined by a value from the Designer.State enumeration.
 * @type {Number}
 * @private
 */
Designer.prototype._state = Designer.State.Standard;
/**
 * Current mouse button states.
 * @type {Object}
 * @private
 */
Designer.prototype._mouseButtons = {
    //TODO: key-ups are missed if the buttons are released outside the canvas, this can mess up the _mouseButtons state
    left: false,
    middle: false,
    right: false,
    toString: function () {
        var buttons = [];
        for (var button in this) {
            if (button == "toString") continue;
            if (this[button]) buttons.push(button);
        }
        return "[" + buttons.join(",") + "]";
    }
};
/**
 * The current mouse position.
 * @type {Point}
 * @private
 */
Designer.prototype._mousePosition = null;
/**
 * The position where dragging was started.
 * @type {Point}
 * @private
 */
Designer.prototype._mouseDragStart = null;
/**
 * The current selection rectangle.
 * @type {Rect}
 * @private
 */
Designer.prototype._selectionRect = null;
/**
 * The number of frames rendered.
 * @type {Number}
 * @private
 */
Designer.prototype._framesRendered = 0;

/**
 * Resets the designer.
 * @public
 */
Designer.prototype.Reset = function () {
    this._objects = [];
    this._selectedObjects = [];
    this._hoveredObject = null;
    this._setCurrentLayout(null);
};

/**
 * Initializes the button pane by loading its template from the server and injecting it into the DOM.
 * Also initializes all UI elements and events.
 * @private
 */
Designer.prototype._createButtonpane = function () {
    var $this = this;
    $.ajax({
        url: "designer_buttonpane.html",
        success: function (data) {
            $this._container.append(data);
            // find containing element
            var pane = $this._container.find(".buttonpane");
            // prepare buttons
            //TODO: keep the buttons disabled state in sync with the possible operations
            pane.find("#new").button({ icons: { primary: "ui-icon-document" } })
                .click(function() { $this.New(); });
            pane.find("#save").button({ icons: { primary: "ui-icon-pencil" }, disabled: true })
                .click(function() { $this.Save(); });
            pane.find("#saveas").button({ icons: { primary: "ui-icon-disk" } })
                .click(function() { $this.SaveAs(); });
			pane.find("#delete").button({ icons: { primary: "ui-icon-trash" } })
                .click(function() { $this.Delete(); });
            pane.find("#flipSize").button({ icons: { primary: "ui-icon-transfer-e-w" }, text: false })
                .click(function() {
                    var w = $("#width"), h = $("#height");
                    var tmp = w.val();
                    w.val(h.val());
                    h.val(tmp);
                });
            pane.find("#apply").button({ icons: { primary: "ui-icon-check" } })
                .click(function() { $this.ApplyCurrentObject(); });
            pane.find("#debugConsoleClear").button({ icons: { primary: "ui-icon-trash" } })
                .click(function() { $("#debugConsole").text(""); });
            // initialize color picker
            $.minicolors.init();
            // put the whole menu inside an accordion
            pane.accordion({
                heightStyle: "content",
                collapsible: true,
                active: 0
            });
            // keep reference
            $this._buttonpane = pane;
        }
    });
};

/**
 * Toogles visibility of the button pane if called without arguments.
 * If the argument is given, visibility is set to the given value.
 * @param {Boolean} visible
 * @public
 */
Designer.prototype.ToggleButtonpane = function(visible) {
    if (arguments.length == 0) {
        this._buttonpane.toggle();
    } else {
        this._buttonpane.toggle(visible);
    }
};

/**
 * Initializes or refreshes the resizing element of the designer.
 * @public
 */
Designer.prototype.RefreshResizer = function() {
    var $this = this;
    var grid = this._options.grid;
    var resize = function(event, ui) {
        if ($this.SetSize(new Size(ui.size.width, ui.size.height).Scale(1/grid, true))) {
            // redraw if dimensions have changed
            $this.Render();
        }
    };
    this._resizer.resizable({
        grid: grid,
        minWidth: 10 * grid,
        minHeight: 10 * grid,
        helper: "resizer-helper",
        start: function(event, ui) {
            $this._resizer.addClass("resizer-helper");
        },
        resize: resize,
        stop: function(event, ui) {
            $this._resizer.removeClass("resizer-helper");
            resize(event, ui);
        }
    });
    function toggleClass(selector, css) {
        $this._resizer.find(selector).unbind(".designer")
            .bind("mouseenter.designer", function() { $this._resizer.addClass(css); })
            .bind("mouseleave.designer", function() { $this._resizer.removeClass(css); });
    }
    toggleClass(".ui-resizable-e", "resizer-helper-e");
    toggleClass(".ui-resizable-s", "resizer-helper-s");
    toggleClass(".ui-resizable-se", "resizer-helper-e resizer-helper-s");
};

/**
 * Resizes the canvas to given size in grid-coordinates.
 * @param {Size} size The size which should be applied.
 * @return {Boolean} True if the size was adjusted, otherwise false, i.e. no changes needed.
 * @public
 */
Designer.prototype.SetSize = function (size) {
    // use current dimensions if called without argument
    if (arguments.length == 0) {
        size = new Size(this._options.width, this._options.height);
    }
    // remember size in grid units
    this._options.width = size.width;
    this._options.height = size.height;
    // scale size to pixel units
    size.Scale(this._options.grid);
    // compensate for last grid line
    size.width++;
    size.height++;
    // adjust resizer
    this._resizer.width(size.width);
    this._resizer.height(size.height);
    this.RefreshResizer();
    // check if nothing has changed
    if (this._canvas.width == size.width && this._canvas.height == size.height) {
        return false;
    }
    // set canvas size in pixels
    this._canvas.width = size.width;
    this._canvas.height = size.height;
    return true;
};

/**
 * Calculates the size needed for current layout and adjusts the canvas size accordingly.
 * @return {Boolean} True if the size was adjusted, otherwise false, i.e. no changes needed.
 * @public
 */
Designer.prototype.AutoSize = function () {
    // adjust canvas size, e.g. for changed grid-size
    // prevents collapsing to a single cell (width x height: 1x1)
    if (this._objects == null || this._objects.length == 0) {
        return this.SetSize();
    }
    // ** DEBUG, as long as server output is nonsense
    var width = 0;
    var height = 0;
    // find min width and height needed
    for (var i = 0; i < this._objects.length; i++) {
        var obj = this._objects[i];
        if (obj.left + obj.width > width) {
            width = obj.left + obj.width;
        }
        if (obj.top + obj.height > height) {
            height = obj.top + obj.height;
        }
    }
    // **
    // apply correct size
    var space = 2 * this._options.spacing;
    return this.SetSize(new Size(width + space, height + space));
};

/**
 * Searches for an object within the current layout which contains the given point.
 * @param {Point} point The point in pixel-coordinates.
 * @return {Building} The object at the given point or null if nothing is found.
 * @private
 */
Designer.prototype._findObjectAtPosition = function(point) {
    var p = point.Copy().Scale(1/this._options.grid);
    for (var i = 0; i < this._objects.length; i++) {
        if (this._objects[i].Rect().ContainsPoint(p)) {
            return this._objects[i];
        }
    }
    return null;
};

/**
 * Resets the designer and parses the layout received from the server.
 * @param layout The layout to be parsed.
 * @private
 */
Designer.prototype._parseLayout = function (layout) {
    this.Reset();
    this._setCurrentLayout(layout);
    // parse objects retrieved from service
    this._objects = [];
    for (var i = 0; i < layout.objects.length; i++) {
        this._objects.push(Building.FromObject(layout.objects[i]));
    }
    // if auto-sizing adjust canvas size to fit the layout
    if (this._options.autoSize) {
        this.AutoSize();
    }
    // render the new layout
    this.Render();
};

/**
 * Resets the designer to create a new layout.
 * @public
 */
Designer.prototype.New = function () {
    this.Reset();
    this.Render();
    this._options.layoutReset();
};

/**
 * Loads the layout with the given ID from the server.
 * @param id The id of the layout to load.
 * @public
 */
Designer.prototype.Load = function (id) {
    // load file from url and parse as json
    var $this = this;
    Rest("GET", this._options.serviceUrl + "layout/" + id, null,
        function (data) {
            $this._parseLayout(data);
        }
    );
};

/**
 * Saves modifications to the current layout to the server. Layout must already exist on the server to succeed.
 * @public
 */
Designer.prototype.Save = function () {
    //TODO: implement Save()
};

/**
 * Saves an independent copy of the current layout on the server.
 * @public
 */
Designer.prototype.SaveAs = function () {
    // validation: empty layout
    if (this._objects == null || this._objects.length == 0) {
        alert("Nothing placed.");
        return;
    }
    // validation: no name set
    var name = this._buttonpane.find("#layoutName").val();
    if (name == "") {
        alert("No name given.");
        return;
    }
    // POST layout to webservice
    var $this = this;
    Rest("POST",this._options.serviceUrl + "layout",
        "data=" + JSON.stringify({
            name: name,
            objects: this._objects
        }),
        function (data) {
            if (!data.success) {
                alert("save failed");
                return;
            }
            // update layout information
            $this._setCurrentLayout(data.layout);
            // fire changed event
            $this._options.layoutChanged(data.ID);
        });
};

/**
 * Deletes the current layout from the server and clears the designer on success.
 * @public
 */
Designer.prototype.Delete = function () {
    // delete the currently loaded layout
    if (this._layout == null) {
        alert("nothing to delete");
        return;
    }
    //TODO: add confirmation dialog
    var $this = this;
    Rest("DELETE", this._options.serviceUrl + "layout/" + this._layout.ID, null,
        function (data) {
            if (!data.success) {
                alert("deletion failed");
                return;
            }
            // fire deleted event
            $this._options.layoutDeleted($this._layout.ID);
            // reset the editor
            $this.Reset();
			$this.Render();
        });
};

/**
 * Sets the information within the button pane and the current layout to the given data.
 * @param layout Layout information received from webservice or null to clear the current layout.
 * @private
 */
Designer.prototype._setCurrentLayout  = function(layout) {
    this._layout = layout;
    var b = this._buttonpane;
    if (b == null) {
        return;
    }
    // default values to show when no layout is set
    if (layout == null) {
        layout = { name: "", author: "", width: 0, height: 0, created: "", edited: "" };
    }
    // set information
    b.find("#layoutName").val(layout.name);
    b.find("#layoutAuthor").html(layout.author);
    b.find("#layoutSize").html(layout.width + "x" + layout.height);
    b.find("#layoutCreated").html(layout.created);
    b.find("#layoutEdited").html(layout.edited);
};

/**
 * Retrieves current user input and sets the current object, if validation succeeds.
 * @public
 */
Designer.prototype.ApplyCurrentObject = function() {
    this._currentObject = this._getCurrentProperties();
    if (!this._currentObject.IsValid()) {
        //TODO: give nice validation feedback
        alert("object invalid");
        this._currentObject = null;
    }
};

/**
 * Sets the user input within the button pane to the values of the given object.
 * If the button pane is not initialized or the given object is null nothing is done.
 * @param {Building} building The object whichs properties should be used.
 * @private
 */
Designer.prototype._setCurrentProperties = function(building) {
    var b = this._buttonpane;
    if (b == null || building == null) {
        return;
    }
    b.find("#width").val(building.width);
    b.find("#height").val(building.height);
    b.find("#color").val(building.color);
    b.find("#label").val(building.label);
    b.find("#enableLabel")[0].checked = building.enableLabel;
    b.find("#borderless")[0].checked = building.borderless;
    b.find("#road")[0].checked = building.road;
    $.minicolors.refresh();
};

/**
 * Retrieves object properties from the user input through the button pane.
 * @return {Building} An object with its properties set to the user input. Null if the button pane is not initialized.
 * @private
 */
Designer.prototype._getCurrentProperties = function() {
    var bp = this._buttonpane;
    if (bp == null) {
        return null;
    }
    var b = new Building(0, 0,
        Convert.int(bp.find("#width").val()),
        Convert.int(bp.find("#height").val()),
        bp.find("#color").val(),
        bp.find("#label").val()
    );
    b.enableLabel = bp.find("#enableLabel")[0].checked;
    b.borderless = bp.find("#borderless")[0].checked;
    b.road = bp.find("#road")[0].checked;
    return b;
};

/**
 * Moves the current object to the current position of the mouse.
 * The object is moved so that its center is nearest to the mouse.
 * @return {Boolean} True, if the objects position was modified.
 * Otherwise false, i.e. if the object is null or already at the correct position.
 * @private
 */
Designer.prototype._moveCurrentObjectToMouse = function() {
    if (this._currentObject == null) {
        return false;
    }
    var obj = this._currentObject;
    // place centered on mouse position
    var size = obj.Size().Scale(this._options.grid);
    var pos = this._mousePosition.Copy();
    pos.x -= size.width / 2;
    pos.y -= size.height / 2;
    pos.Scale(1/this._options.grid, true);
    if (obj.left != pos.x || obj.top != pos.y) {
        this._currentObject.Position(pos);
        return true;
    }
    return false;
};

/**
 * Tries to place the current object at its current location.
 * Applies collision detection to prevent invalid placement.
 * @return {Boolean} True, if placement was successful. Otherwise false.
 * @private
 */
Designer.prototype._tryPlaceCurrentObject = function() {
    if (this._currentObject != null) {
        // check for collisions
        var rect = this._currentObject.Rect();
        for (var i = 0; i < this._objects.length; i++) {
            if (this._objects[i].Rect().IntersectsWith(rect)) {
                return false;
            }
        }
        var copy = Building.FromObject(this._currentObject);
        // add borderless objects at the start of the array, because they should be drawn first
        if (copy.borderless) {
            this._objects.unshift(copy);
        } else {
            this._objects.push(copy);
        }
        return true;
    }
    return false;
};

/**
 * Registers all events on the canvas.
 * Any default behavior of the events used is prevented.
 * @private
 */
Designer.prototype._registerEvents = function () {
    var $this = this;
    function overwriteEvent(eventName, handler) {
        $($this._canvas).bind(eventName + ".designer", function (e) {
            if (handler) {
                handler.apply($this, [e]);
            }
            e.preventDefault();
            return false;
        });
    }
    // register mouse events
    overwriteEvent("mousedown", this._onMouseDown);
    overwriteEvent("mousemove", this._onMouseMove);
    overwriteEvent("mouseup", this._onMouseUp);
    overwriteEvent("dblclick", this._onMouseDblClick);
    overwriteEvent("mousewheel", this._onMouseWheel);
    overwriteEvent("mouseout", this._onMouseOut);
    overwriteEvent("contextmenu");
    // register keboard events
    overwriteEvent("keydown", this._onKeyDown);
};

/**
 * Unregisters all events from the canvas.
 * @private
 */
Designer.prototype._unregisterEvents = function () {
    // remove all events
    $(this._canvas).unbind(".designer")
};

/**
 * Helper method for mouse event handlers.
 * Keeps track of the current mouse position.
 * @param e The mouse event object.
 * @return {Point} The current mouse position.
 * @private
 */
Designer.prototype._handleMousePosition = function (e) {
    this._mousePosition = new Point(e.offsetX, e.offsetY);
    return this._mousePosition;
};

/**
 * Helper method for mouse event handlers.
 * Keeps track of the current state of all mouse buttons.
 * Needed because mouse events not always contain information about all buttons.
 * @param e The mouse event object.
 * @param {Boolean} pressed Specified whether the event is for mouse-down or -up.
 * @private
 */
Designer.prototype._handleMouseButtons = function (e, pressed) {
    switch (e.which) {
        case MouseButton.Left: this._mouseButtons.left = pressed; break;
        case MouseButton.Middle: this._mouseButtons.middle = pressed; break;
        case MouseButton.Right: this._mouseButtons.right = pressed; break;
    }
};

/**
 * Handles mousedown events on the canvas.
 * @param e The mousedown event object.
 * @private
 */
Designer.prototype._onMouseDown = function (e) {
    // the canvas has to be focused, otherwise key events will not work
    this._canvas.focus();
    var pos = this._handleMousePosition(e);
    var render = this._moveCurrentObjectToMouse();
    this._handleMouseButtons(e, true);
    this.Trace("mousedown " + this._mouseButtons.toString());
    this._mouseDragStart = pos.Copy();
    var buttons = this._mouseButtons;
    if (buttons.left && buttons.right) {
        this._state = Designer.State.DragAllStart;
    } else if (buttons.left && this._currentObject != null) {
        if (this._tryPlaceCurrentObject()) {
            render = true;
        }
    } else if (buttons.left && this._currentObject == null) {
        var obj = this._findObjectAtPosition(pos);
        if (obj == null) {
            // user clicked in empty space: start dragging selecion rectangle
            this._state = Designer.State.SelectionRectStart;
            render = true;
        } else if (!this._controlKeysPressed(e)) {
            // user clicked on object:
            // - if it is selected, start dragging all selected objects
            // - if it is not, start dragging just that single object
            this._state = this._selectedObjects.contains(obj) ?
                Designer.State.DragSelectionStart : Designer.State.DragSingleStart;
        }
    }
    // re-render if necessary
    if (render) {
        this.Render();
    }
};

/**
 * Handles mousemove events on the canvas.
 * @param e The mousemove event object.
 * @private
 */
Designer.prototype._onMouseMove = function (e) {
    var pos = this._handleMousePosition(e);
    var render = this._moveCurrentObjectToMouse();
    this.Trace("mousemove " + this._mouseButtons.toString());
    var dragPos = this._mouseDragStart;
    var buttons = this._mouseButtons;
    // check if user begins to drag
    if (this._state != Designer.State.Standard && (Math.abs(dragPos.x - pos.x) > 1 || Math.abs(dragPos.y - pos.y) > 1)) {
        switch (this._state) {
            case Designer.State.SelectionRectStart:
                this._state = Designer.State.SelectionRect;
                this._selectionRect = Rect.FromPoints(dragPos, pos);
                break;
            case Designer.State.DragSelectionStart:
                this._state = Designer.State.DragSelection;
                break;
            case Designer.State.DragSingleStart:
                this._selectedObjects = [this._findObjectAtPosition(dragPos)];
                this._state = Designer.State.DragSelection;
                break;
            case Designer.State.DragAllStart:
                this._state = Designer.State.DragAll;
                break;
        }
    }
    var grid = this._options.grid;
    // drag delta
    var dis;
    function GetDragDelta() {
        var dis = pos.Copy();
        dis.x -= dragPos.x;
        dis.y -= dragPos.y;
        dis.Scale(1/grid, true);
        return dis;
    }
    var i, j, obj;
    switch (this._state) {
        default:
            // highlight hovered object if not currently placing an object
            if (this._currentObject == null) {
                obj = this._findObjectAtPosition(pos);
                if (this._hoveredObject != obj) {
                    this._hoveredObject = obj;
                    render = true;
                }
            } else if (this._hoveredObject != null) {
                this._hoveredObject = null;
                render = true;
            }
            // place objects when moving the mouse while the left button is pressed
            if (buttons.left && this._currentObject != null) {
                if (this._tryPlaceCurrentObject()) {
                    render = true;
                }
            }
            break;
        case Designer.State.SelectionRect:
            if (this._controlKeysPressed(e)) {
                // remove previously selected by the selection rect
                // iterate backwards, because the array is modifed during the loop and indexes would shift
                for (j = this._selectedObjects.length-1; j >= 0; j--) {
                    obj = this._selectedObjects[j];
                    if (obj.Rect().Scale(this._options.grid).IntersectsWith(this._selectionRect)) {
                        this._selectedObjects.remove(obj);
                    }
                }
            } else {
                this._selectedObjects = [];
            }
            // adjust rect
            //TODO: snap selection rect to grid and reduce redraws?
            this._selectionRect = Rect.FromPoints(this._mouseDragStart, this._mousePosition);
            // select intersecting objects
            for (j = 0; j < this._objects.length; j++) {
                obj = this._objects[j];
                if (!this._selectedObjects.contains(obj) &&
                    obj.Rect().Scale(this._options.grid).IntersectsWith(this._selectionRect)) {
                    this._selectedObjects.push(obj);
                }
            }
            render = true;
            break;
        case Designer.State.DragSelection:
            // drag selection objects around, always checking for collisions
            dis = GetDragDelta();
            // check if the mouse has moved at least one grid cell in any direction
            if (dis.x == 0 && dis.y == 0) {
                break;
            }
            // move selected objects
            for (i = 0; i < this._selectedObjects.length; i++) {
                obj = this._selectedObjects[i];
                obj.left += dis.x;
                obj.top += dis.y;
            }
            // check for collisions with unselected objects
            var collision = false;
            for (i = 0; i < this._objects.length; i++) {
                if (this._selectedObjects.contains(this._objects[i])) {
                    continue;
                }
                var rect = this._objects[i].Rect();
                for (j = 0; j < this._selectedObjects.length; j++) {
                    if (this._selectedObjects[j].Rect().IntersectsWith(rect)) {
                        collision = true;
                        break;
                    }
                }
                if (collision){
                    break;
                }
            }
            if (collision) {
                // roll back movement on collision
                for (i = 0; i < this._selectedObjects.length; i++) {
                    obj = this._selectedObjects[i];
                    obj.left -= dis.x;
                    obj.top -= dis.y;
                }
            } else {
                // adjust the drag start to compensate the amount we already moved
                dis.Scale(grid);
                this._mouseDragStart.x += dis.x;
                this._mouseDragStart.y += dis.y;
                render = true;
            }
            break;
        case Designer.State.DragAll:
            dis = GetDragDelta();
            // check if the mouse has moved at least one grid cell in any direction
            if (dis.x == 0 && dis.y == 0) {
                break;
            }
            // move all objects
            for (i = 0; i < this._objects.length; i++) {
                this._objects[i].left += dis.x;
                this._objects[i].top += dis.y;
            }
            // adjust the drag start to compensate the amount we already moved
            dis.Scale(grid);
            this._mouseDragStart.x += dis.x;
            this._mouseDragStart.y += dis.y;
            render = true;
            break;
    }
    // re-render if necessary
    if (render) {
        this.Render();
    }
};

/**
 * Handles mouseup events on the canvas.
 * @param e The mouseup event object.
 * @private
 */
Designer.prototype._onMouseUp = function (e) {
    var pos = this._handleMousePosition(e);
    this._handleMouseButtons(e, false);
    this.Trace("mouseup " + this._mouseButtons.toString());
    var buttons = this._mouseButtons;
    if (this._state == Designer.State.DragAll) {
        if (!buttons.left || !buttons.right) {
            this._state = Designer.State.Standard;
        }
        return;
    }
    var obj;
    // left button up
    if (e.which == MouseButton.Left && this._currentObject == null) {
        switch (this._state) {
            default:
                // clear selection of no control key is pressed
                if (!this._controlKeysPressed(e)) {
                    this._selectedObjects = [];
                }
                obj = this._findObjectAtPosition(pos);
                if (obj != null) {
                    // user clicked an object: (de-)select it
                    if (this._selectedObjects.contains(obj)) {
                        this._selectedObjects.remove(obj);
                    } else {
                        this._selectedObjects.push(obj);
                    }
                }
                break;
            case Designer.State.SelectionRect:
            case Designer.State.DragSelection:
                break;
        }
        // return to standard state
        this._state = Designer.State.Standard;
    }
    // right button up
    if (e.which == MouseButton.Right && this._state == Designer.State.Standard) {
        if (this._currentObject == null) {
            obj = this._findObjectAtPosition(pos);
            if (obj == null) {
                if (!this._controlKeysPressed(e)) {
                    // use right clicked in empty space: clear selection
                    this._selectedObjects = [];
                }
            } else {
                // user right clicked an existing object: remove it
                this._objects.remove(obj);
                this._selectedObjects.remove(obj);
            }
        } else {
            // cancel placement
            this._currentObject = null;
        }
    }
    // rotate current object
    if (e.which == MouseButton.Middle && this._currentObject != null) {
        this._currentObject.FlipSize();
    }
    this.Render();
};

/**
 * Handles doubleclick events on the canvas.
 * @param e The dblclick event object.
 * @private
 */
Designer.prototype._onMouseDblClick = function (e) {
    var pos = this._handleMousePosition(e);
    this.Trace("mousedblclick " + this._mouseButtons.toString());
    if (this._currentObject == null) {
        this._setCurrentProperties(this._findObjectAtPosition(pos));
        this.ApplyCurrentObject();
        this.Render();
    }
};

/**
 * Handles mousewheel events on the canvas.
 * @param e The mousewheel event object.
 * @private
 */
Designer.prototype._onMouseWheel = function(e) {
    this.Trace("mousewheel");
    var delta = event.wheelDelta/50 || -event.detail;
    this._options.grid = Math.round(this._options.grid * (delta < 0 ? 1/this._options.zoomSpeed : this._options.zoomSpeed));
    if (this._options.grid < 1)
    {
        this._options.grid = 1;
    }
    if (this._options.autoSize) {
        this.AutoSize();
    }
    this.Render();
};

/**
 * Handles mouseout events on the canvas.
 * @param e The mouseout event object.
 * @private
 */
Designer.prototype._onMouseOut = function (e) {
    this.Trace("mouseout");
    this._hoveredObject = null;
    this._mousePosition = null;
    this.Render();
};

/**
 * Handles keydown events on the canvas.
 * @param e The keydown event object.
 * @private
 */
Designer.prototype._onKeyDown = function (e) {
    this.Trace("keydown [" + e.keyCode + "]");
    switch (e.keyCode) {
        // delete key
        case 46:
            if (this._selectedObjects.length == 0) {
                break;
            }
            // delete currently selected objects
            for (var i = 0; i < this._selectedObjects.length; i++) {
                this._objects.remove(this._selectedObjects[i]);
            }
            this._selectedObjects = [];
            this.Render();
            break;
    }
};

/**
 * Checks if any of the controls keys (ctrl or shift) is pressed for the given event.
 * @param e The event object.
 * @return {Boolean} True if ctrl, shift or both buttons are pressed. Otherwise false.
 * @private
 */
Designer.prototype._controlKeysPressed = function (e) {
    return e.ctrlKey || e.shiftKey;
};

/**
 * Main rendering method. Clears the canvas and re-renders the whole frame.
 * @public
 */
Designer.prototype.Render = function () {
    // shorthand definitions
    var o = this._options;
    var ctx = this._ctx;
    // reset transform
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    // clear the whole canvas (transparent)
    ctx.clearRect(0, 0, this._canvas.width, this._canvas.height);
    // render grid
    if (o.drawGrid) {
        this._renderGrid();
    }
    // skip the rest if no data is present
    if (this._objects == null) {
        return;
    }
    var i;
    // draw current objects
    for (i = 0; i < this._objects.length; i++) {
        this._renderObject(this._objects[i]);
    }
    // draw highlights
    var obj;
    for (i = 0; i < this._selectedObjects.length; i++) {
        obj = this._selectedObjects[i];
        ctx.lineWidth = 2;
        ctx.strokeStyle = "#00FF00";
        this._strokeRect(obj.Rect().Scale(o.grid));
    }
    // draw hovered object highlight
    if (this._hoveredObject != null) {
        ctx.lineWidth = 2;
        ctx.strokeStyle = "#FFFF00";
        this._strokeRect(this._hoveredObject.Rect().Scale(o.grid));
    }
    // draw "ghost" if currently placing a new object
    if (this._currentObject != null) {
        this._renderObject(this._currentObject);
    }
    if (this._state == Designer.State.SelectionRect) {
        ctx.fillStyle = "#FFFF00";
        ctx.globalAlpha = 0.4;
        this._fillRect(this._selectionRect);
        ctx.globalAlpha = 1;
    }
    // output debug information
    for (var s in Designer.State) {
        if (this._state == Designer.State[s]) {
            $("#debugState").html(s);
        }
    }
    $("#debugFrameCount").html(++this._framesRendered);
};

/**
 * Renders the grid to the canvas, uses the grid-size set within the options.
 * @private
 */
Designer.prototype._renderGrid = function () {
    var ctx = this._ctx;
    var grid = this._options.grid;
    var maxWidth = this._options.width * grid;
    var maxHeight = this._options.height * grid;
    // translate half pixel to the right and down to achieve pixel perfect lines
    ctx.translate(0.5, 0.5);
    ctx.strokeStyle = "#000000";
    ctx.lineWidth = 1;
    ctx.beginPath();
    for (var x = 0; x < this._canvas.width; x += grid) {
        // vertical lines
        ctx.moveTo(x, 0);
        ctx.lineTo(x, maxHeight);
    }
    for (var y = 0; y < this._canvas.height; y += grid) {
        // horizontal lines
        ctx.moveTo(0, y);
        ctx.lineTo(maxWidth, y);
    }
    ctx.stroke();
    ctx.translate(-0.5, -0.5);
};

/**
 * Renders the given object to the canvas.
 * @param {Building} obj The object to render.
 * @private
 */
Designer.prototype._renderObject = function (obj) {
    var ctx = this._ctx;
    ctx.lineWidth = 1;
    ctx.fillStyle = obj.color;
    ctx.strokeStyle = "#000000";
    var rect = obj.Rect().Scale(this._options.grid);
	// fix to overlap adjacent lines of the grid on the bottom and right side
	if (obj.borderless) {
		rect.width++;
		rect.height++;
	}
    this._fillRect(rect);
    if (!obj.borderless) {
        this._strokeRect(rect);
    }
    ctx.fillStyle = "#000000";
    this._renderText(obj.label, obj.Position(), obj.Size());
};

/**
 * Renders a string centered within a rectangle to the canvas.
 * @param {String} text The string to be rendered.
 * @param {Point} point The point to render at, given in grid-coordinates.
 * @param {Size} size The available space for rendering, given in grid-coordinates.
 * @private
 */
Designer.prototype._renderText = function(text, point, size) {
    var ctx = this._ctx;
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    var p = point.Copy().Scale(this._options.grid);
    var s = size.Copy().Scale(this._options.grid);
    ctx.fillText(text, p.x + s.width/2, p.y + s.height/2, s.width);
};

/**
 * Draws a stroke rectangle to the canvas, fixing blurry lines for odd line widths
 * by translating half a pixel to the bottom right.
 * @param {Rect} rect
 * @private
 */
Designer.prototype._strokeRect = function (rect) {
    if (this._ctx.lineWidth % 2 == 0) {
        this._ctx.strokeRect(rect.left, rect.top, rect.width, rect.height);
    } else {
        // corrects blurry lines caused by lines between two pixels
        this._ctx.translate(0.5, 0.5);
        this._ctx.strokeRect(rect.left, rect.top, rect.width, rect.height);
        this._ctx.translate(-0.5, -0.5);
    }
};

/**
 * Draws a filled rectangle to the canvas.
 * @param {Rect} rect
 * @private
 */
Designer.prototype._fillRect = function (rect) {
    this._ctx.fillRect(rect.left, rect.top, rect.width, rect.height);
};

/**
 * Internal counter for the debugging console. Keeps track of how often the same message was repeated.
 * @type {Number}
 * @private
 */
Designer.prototype._traceCounter = 0;

/**
 * Appends a message to the debug console.
 * @param {String} message
 * @public
 */
Designer.prototype.Trace = function(message) {
//    trace(message);
    var con = $("#debugConsole");
    var total = con.text().split("\n");
    // append counter to message if sent multiple times, otherwise just add it
    if (total.last().startsWith(message)) {
        var str = total.pop();
        str = str.slice(0, message.length);
        str += "(" + ++this._traceCounter + ")";
        total.push(str);
    } else {
        total.push(message);
        this._traceCounter = 0;
    }
    // remove first lines if empty
    while (total[0] == "") {
        total.shift();
    }
    // remove first lines, when there are too many
    var max = 50;
    if (total.length > max) {
        total = total.slice(total.length - max);
    }
    // set text and scroll down
    con.text(total.join("\n"));
    con.scrollTop(con[0].scrollHeight - con.height());
};
