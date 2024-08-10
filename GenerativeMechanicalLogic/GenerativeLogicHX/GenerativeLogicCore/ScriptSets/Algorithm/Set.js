

function Set() {
    this.control = new Control();
};
Set.prototype = {

    Union: function (set1, set2, comparer) {
        var t = this;
        var map = new Map();
        control.ProcessArray(set1, item => { map.set(item, item); });
        control.ProcessArray(set2, item => { map.set(item, item); });
        var result = [];
        map.forEach((value, key, map) => { result.push(key); });
        return result;
    },

    Intersect: function (set1, set2, comparer) {
        var t = this;
        var map = new Map();
        var result = [];
         control.ProcessArray(set1, item => { map.set(item, item); });
         control.ProcessArray(set2, item => {
            if (map.has(item)) { result.push(item); }
        });
        return result;
    },

    Subtract: function (set, subtraction, comparer) {
        var t = this;
        var map = new Map();
        var result = [];
         control.ProcessArray(subtraction, item => { map.set(item, item); });
         control.ProcessArray(set, item => {
            if (!map.has(item)) { result.push(item); }
        });
        return result;
    },

    //Complements set with the properties in complement
    Complement: function (set, complement) {
        var t = this;
        if (set == null) { set = {}; }
        var pairs =  control.GetObjectMemberNameValuePairs(complement);
         control.ProcessArray(pairs, pair => {
            if (set[pair.name] == null) { set[pair.name] = pair.value; }
        });
        return set;
    },

    Clone: function (set) {
        var t = this;
        var o = {};
        return t.Complement(o, set);
    },

    InitializeObject: function (object, parameters, defaults) {
        var t = this;
        var x = t.Complement(parameters, defaults);
        t.Complement(object, x);
    },

    InitializeOobject: function (object, parameters, defaults) {
        var t = this;
        t.InitializeObject(object, parameters, defaults);
    },

    IsIn: function (item, values, comparer) {
        var t = this;
        return false ===  control.ProcessArray(values, value => {
            if (comparer(item, value)) { return false; }
        });
    }
};
