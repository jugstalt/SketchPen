(function ($) {
    "use strict";
    $.fn.sketchPenCode_globals = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_globals');
        }
    };

    let defaults = {
        $toolbar: null,
    };

    let methods = {
        init: function (options) {
            let settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        },
        refresh: function (options) {
            refresh($(this));
        },
        currentValue: function (options) {
            let $select = $(this).find('.sketchpen-code-globals-select');

            return toGlobalsValue($select.val());
        }
    };

    let initUI = function (parent, options) {
        let $parent = $(parent);

        let $toolbar = $("<div>")
            .addClass('sketchpen-code-globals-toolbar')
            .appendTo($parent);

        $("<div>")
            .addClass('sketchpen-code-globals-toolbutton add')
            .appendTo($toolbar);

        $("<div>")
            .addClass('sketchpen-code-globals-toolbutton edit')
            .appendTo($toolbar);

        let $selectHodler = $("<div>")
            .addClass('sketchpen-code-globals-select-holder')
            .appendTo($parent);

        let $select = $("<select>")
            .addClass('sketchpen-code-globals-select')
            .appendTo($selectHodler);

        refresh($parent);
    };

    let refresh = function ($parent) {
        let $select = $parent.find('.sketchpen-code-globals-select');

        $select.empty();
        sketchPenCode.api.getGlobals(function (filenames) {
            $.each(filenames, function (i, filename) {
                $("<option>")
                    .attr('value', filename)
                    .text(filename)
                    .appendTo($select);

            });
        });
    };

    let toGlobalsValue = function (filename) {
        if (filename.indexOf('_') == 0 &&
            filename.indexOf('.globals') === filename.length - '.globals'.length) {
            return filename.substr(1, filename.length - '.globals'.length - 1);
        }

        return '';
    };
})(jQuery);
