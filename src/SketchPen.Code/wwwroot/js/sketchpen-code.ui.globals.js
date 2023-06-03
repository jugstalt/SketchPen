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
            $.error('Method ' + method + ' does not exist on jExt.sketchPenCode_globals');
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
        let $parent = $(parent)
            .addClass('sketchpen-code-globals-holder');

        let $toolbar = $("<div>")
            .addClass('sketchpen-code-globals-toolbar')
            .appendTo($parent);

        $("<div>")
            .addClass('sketchpen-code-globals-toolbutton add')
            .appendTo($toolbar)
            .click(function (e) {
                e.stopPropagation();
                var $this = $(this);

                sketchPenCode.ui.prompt('New Globals', 'Enter the name of a new globals (without the .globals extension)',
                    function (val) {
                        val = '_' + val + sketchPenCode.globalsFileExt();

                        sketchPenCode.api.createFile(val, function (result) {
                            if (result.success == true) {
                                refresh($this.closest('.sketchpen-code-globals-holder'));
                                if (result.route) {
                                    sketchPenCode.events.fire('open-file', {
                                        route: result.route
                                    });
                                }
                            } else {
                                sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                            }
                        });
                    }, 'New globals name...');
            });

        $("<div>")
            .addClass('sketchpen-code-globals-toolbutton edit')
            .appendTo($toolbar)
            .click(function (e) {
                e.stopPropagation();

                var $globalsHolder = $(this).closest('.sketchpen-code-globals-holder');
                let $select = $globalsHolder.find('.sketchpen-code-globals-select');
                var route = sketchPenCode.id() + '/' + $select.val();

                sketchPenCode.events.fire('open-file', {
                    route: route
                });
            });

        $("<div>")
            .addClass('sketchpen-code-globals-toolbutton remove')
            .appendTo($toolbar)
            .click(function (e) {
                e.stopPropagation();

                var $globalsHolder = $(this).closest('.sketchpen-code-globals-holder');
                let $select = $globalsHolder.find('.sketchpen-code-globals-select');

                sketchPenCode.events.fire('delete-document', { file: $select.val() });
            });

        let $selectHodler = $("<div>")
            .addClass('sketchpen-code-globals-select-holder')
            .appendTo($parent);

        $("<select>")
            .addClass('sketchpen-code-globals-select')
            .appendTo($selectHodler);

        refresh($parent);

        sketchPenCode.events.on('document-deleted', function (channel, args) {
            if (args && args.route &&
                args.route.indexOf(sketchPenCode.globalsFileExt()) === args.route.length - sketchPenCode.globalsFileExt().length) {
                refresh($parent);
            }
        });
    };

    let refresh = function ($parent) {
        let $select = $parent.find('.sketchpen-code-globals-select');

        $select.empty();
        sketchPenCode.api.getGlobals(function (filenames) {
            $.each(filenames, function (i, filename) {
                $("<option>")
                    .attr('value', filename)
                    .text(toGlobalsValue(filename) || '<globals>')
                    .appendTo($select);

            });
        });
    };

    let toGlobalsValue = function (filename) {
        if (filename.indexOf('_') == 0 &&
            filename.indexOf('.globals') === filename.length - sketchPenCode.globalsFileExt().length) {
            return filename.substr(1, filename.length - sketchPenCode.globalsFileExt().length - 1);
        }

        return '';
    };
})(jExt);
