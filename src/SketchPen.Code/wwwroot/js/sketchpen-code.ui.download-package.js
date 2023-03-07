(function ($) {
    "use strict";
    $.fn.sketchPenCode_download_package = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_download_package');
        }
    };

    let defaults = {
        
    };

    let methods = {
        init: function (options) {
            let settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        }
    };

    let initUI = function (parent, options) {
        let $parent = $(parent);

        _appendLabel($parent, 'Composer:');
        let $composers = $("<select>")
            .addClass('sketchpen-code-input composer')
            .appendTo($parent);

        sketchPenCode.api.getComposers(function (result) {
            for (var composer of result) {
                $("<option>")
                    .attr('value', composer)
                    .text(composer)
                    .appendTo($composers);
            }
        });

        _appendLabel($parent, 'Sizes:');
        let $sizesInput = $("<input>")
            .addClass('sketchpen-code-input sizes')
            .attr('placeholder', 'sizes eg: 16,32,64')
            .appendTo($parent);

        _appendLabel($parent, 'Resolutions [dpi]:');
        let $resolutionsInput = $("<input>")
            .addClass('sketchpen-code-input resolutions')
            .attr('placeholder', 'dpi list eg: 96,144,192')
            .appendTo($parent);

        _appendLabel($parent, "Styles (Globals):")
        let $globalsList = $("<ul>")
            .addClass('sketchpen-code-globals-list styles')
            .appendTo($parent);

        sketchPenCode.api.getGlobals(function (result) {
            for (var filename of result) {
                let $item = $("<li>")
                    .addClass('sketchpen-code-globals-list-item')
                    .attr('data-value', _toGlobalsValue(filename))
                    .text(_toGlobalsValue(filename) || '(default)')
                    .appendTo($globalsList)
                    .click(function (e) {
                        $(this).toggleClass('checked');
                    });

                $("<div>")
                    .addClass('checkbox')
                    .appendTo($item);
            }
        });

        let $iframe = $("<iframe>")
            .css({ width: 0, height: 0, border: 'none' })
            .appendTo($parent)
            .on('load', function () {
                alert('loaded');
            });

        $("<button>")
            .addClass('sketchpen-code-button')
            .text("Compose Package")
            .appendTo($parent)
            .click(function () {
                var settings = _collectSettings($(this).parent());
                console.log(settings);

                let cmd = '/package';
                let url = sketchPenCode.targetUrl() + cmd + '?id=' + sketchPenCode.id() + '&composer=' + settings.composer + '&sizes=' + settings.sizes + '&resolutions=' + settings.resolutions + '&styles=' + settings.styles.toString();

                $("<a>")
                    .attr('href', url)
                    .appendTo($parent)
                    .on('load', function () { alert(1); })
                    [0].click();
                    //.trigger('click');
                //$iframe.attr('src', url);
            });
    };

    let _collectSettings = function ($parent) {
        var styles = [];
        $parent.find('.sketchpen-code-globals-list-item.checked').map(function () { styles.push($(this).attr('data-value')); });

        return {
            composer: $parent.children('.composer').val(),
            sizes: $parent.children('.sizes').val(),
            resolutions: $parent.children('.resolutions').val(),
            styles: styles.toString()
        };
    }

    let _appendLabel = function ($parent, label) {
        $("<div>")
            .addClass('sketchpen-code-label')
            .text(label)
            .appendTo($parent);
    };

    let _toGlobalsValue = function (filename) {
        if (filename.indexOf('_') == 0 &&
            filename.indexOf('.globals') === filename.length - sketchPenCode.globalsFileExt().length) {
            return filename.substr(1, filename.length - sketchPenCode.globalsFileExt().length - 1);
        }

        return '';
    };
})(jQuery);