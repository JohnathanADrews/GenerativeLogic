function DataTree() {
    this.root = { children: {}, open: true, objects: {} };
    this.maxItems = 2;
    this.options = { caseSensitive: true };
    this.nodes = {};
}

DataTree.prototype = {

    SetAddMode: function (options) {
        var t = this;
        if (options.caseSensitive != null) { t.options.caseSensitive = options.caseSensitive };
    },

    AddNode: function (name, object) {
        if (name == null || name.length == 0) {
            return;
        }
        var t = this;
        t.nodes[name] = object;
        var key = name;
        if (!t.options.caseSensitive) {
            key = name.toLowerCase();
        }
        var node = t.root;
        var char = '';
        for (var i = 0; i < key.length; i++) {
            var properties = Object.getOwnPropertyNames(node.objects);
            if (node.open) {
                if (properties.length < t.maxItems) {
                    node.objects[name] = object;
                }
                else {
                    node.open = false;
                    var objects = {};
                    for (var j = 0; j < properties.length; j++) {
                        objects[properties[j]] = node.objects[properties[j]];
                    }
                    node.objects = {};
                    for (var j = 0; j < properties.length; j++) {
                        t.AddNode(properties[j], objects[properties[j]]);
                    }
                    t.AddNode(name, object);
                }
                return;
            }
            else if (i + 1 == key.length) {
                node.objects[name] = object;
                return;
            }
            char = key[i];
            if (node.children[char] == null) {
                node.children[char] = { children: {}, open: true, objects: {} };
            }
            node = node.children[char];
        }
        node.objects[name] = object;
    },

    GetMatched: function (prefix, options) {
        var t = this;
        var key = prefix;
        if (key == null || key.length == 0) {
            key = "";
        }
        if (options.caseSensitive == null) {
            options.caseSensitive = true;
        }
        if (options.count == null) {
            options.count = 1;
        }
        if (!t.options.caseSensitive) {
            key = key.toLowerCase();
        }
        var node = t.root;
        var result = [];
        var char = '';

        for (var i = 0; i < key.length; i++) {
            if (node == null || node.open) {
                //return result;
                break;
            }
            char = key[i];
            node = node.children[char];
        }

        var traverser = new DynamorphTreeTraverser();
        traverser.TraverseTree(TreeTraverser.PLROrder, node,
            function (tnode) {
                var children = [];
                if (tnode == null) {
                    return children;
                }
                var props = Object.getOwnPropertyNames(tnode.children);
                for (var i = 0; i < props.length; i++) { children.push(tnode.children[props[i]]); }
                return children;
            },
            function (tnode) {
                if (tnode == null) {
                    return true;
                }
                var sp = Object.getOwnPropertyNames(tnode.objects);
                for (var i = 0; i < sp.length; i++) {
                    var match = false;
                    if (options.caseSensitive) {
                        match = sp[i].lastIndexOf(key) == 0;
                    }
                    else {
                        match = sp[i].toLowerCase().lastIndexOf(key) == 0;
                    }
                    if (key == "") { match = true; }
                    if (match) {
                        result.push({ name: sp[i], object: tnode.objects[sp[i]] });
                        if (result.length >= options.count) { return false; }
                    }
                }
                return true;
            });
        return result;
    },

    GetMatchedValues: function (prefix, options) {
        var t = this;
        var matches = t.GetMatched(prefix, options);
        var result = [];
        for (var i = 0; i < matches.length; i++) {
            result.push(matches[i].object);
        }
        return result;
    },

    //Returns true iff the segment of the source string given by begin and end, inclusive, is contained in the tree. 
    HasMatch: function (source, begin, end) {
        var t = this;
        if (begin > end) { return false; }
        if (t.root.children[begin] == null) { return false; };
        var node = t.root.children[begin];
        for (var i = begin + 1; i <= end; i++) {
            if (node.children[source[i]] == null) { return false; }
            node = node.children[source[i]];
        }
        return true;
    },

    GetReader: function () {

    },

    GetNodes: function () { return this.nodes; },

    GetNodeList: function () {
        var t = this;
        var result = [];
        for (var key in t.nodes) {
            if (!t.nodes.hasOwnProperty(key)) { continue; }
            result.push(t.nodes[key]);
        }
        return result;
    }
};

function DataTreeReader() {
    this.tree = null;
    this.node = null;
    this.lastMatch = -1;
};
DataTreeReader.prototype = {

    Reset: function () {
        var t = this;
        t.node = root;
        t.lastMatch = -1;
    },

    Next: function (c) {
        var t = this;
        if (t.node == null) { return; }
        if (t.node.children[c] == null) { return false; }
        t.node = t.node.children[c];
        return true;
    }

}