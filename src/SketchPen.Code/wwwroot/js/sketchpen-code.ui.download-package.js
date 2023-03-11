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
                    .attr('value', composer.type)
                    .text(composer.name)
                    .appendTo($composers);
            }
        });

        _appendLabel($parent, 'Sizes:');
        let $sizesList = $("<ul>")
            .addClass('sketchpen-code-sizes-list')
            .appendTo($parent);
        for (var size of [16, 26, 32, 48, 64, 86, 128, 192, 256]) {
            var $item = $("<li>")
                .addClass('sketchpen-code-sizes-list-item')
                .attr('data-value', size)
                .text(size)
                .appendTo($sizesList)
                .click(function (e) {
                    $(this).toggleClass('checked');
                });

            $("<div>")
                .addClass('checkbox')
                .appendTo($item);
        }

        let $sizesInput = $("<input>")
            .addClass('sketchpen-code-input custom-sizes')
            .attr('placeholder', 'more sizes eg: 50,70,90')
            .appendTo($("<li>")
                .addClass('sketchpen-code-sizes-list-item custom')
                .appendTo($sizesList));

        _appendLabel($parent, 'Resolutions [dpi]:');
        let $resolutions = $("<select>")
            .addClass('sketchpen-code-input resolutions')
            .attr('placeholder', 'dpi list eg: 96,144,192')
            .appendTo($parent);

        let res = [];
        for (var i = 96; i <= 192; i += 48) {
            res.push(i);
            $("<option>").attr('value', res.toString()).text(res.toString()).appendTo($resolutions);
        }

        _appendLabel($parent, "Styles (Globals):")
        let $globalsList = $("<ul>")
            .addClass('sketchpen-code-globals-list styles')
            .appendTo($parent);

        sketchPenCode.api.getGlobals(function (result) {
            for (var filename of result) {
                let $item = $("<li>")
                    .addClass('sketchpen-code-globals-list-item')
                    .attr('data-value', _toGlobalsValue(filename))
                    .attr('title', _toGlobalsValue(filename))
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
                //alert('loaded');
            });

        $("<button>")
            .addClass('sketchpen-code-button')
            .text("Compose Package")
            .appendTo($parent)
            .click(function () {
                var $button = $(this);
                
                if ($button.hasClass('loading'))
                    return;
                    
                $button.addClass('loading');

                var settings = _collectSettings($(this).parent());
                //console.log(settings);

                let url = sketchPenCode.targetUrl() + '/package?id=' + sketchPenCode.id() + '&composer=' + settings.composer + '&sizes=' + settings.sizes + '&resolutions=' + settings.resolutions + '&styles=' + settings.styles.toString();

                $.ajax({
                    url: url,
                    success: function (result) {
                        console.log(result);
                        $iframe.attr('src', sketchPenCode.targetUrl() + '/DownloadTempFile?tempFilename=' + result.tempFilename);

                        $button.removeClass('loading');
                    },
                    error: function () {
                        $button.removeClass('loading');
                    }
                });
            });
    };

    let _collectSettings = function ($parent) {
        // Collect styles
        var styles = [];
        $parent.find('.sketchpen-code-globals-list-item.checked').map(function () { styles.push($(this).attr('data-value')); });

        // Collect sizes
        var sizes = [];
        $parent.find('.sketchpen-code-sizes-list-item.checked').map(function () { sizes.push($(this).attr('data-value')) });
        // Collect customSizes
        if ($parent.find('.custom-sizes').val()) {
            for (var size of $parent.find('.custom-sizes').val().split(',')) {
                size = size.trim();

                if ($.inArray(size, sizes) < 0) {
                    sizes.push(size);
                }
            }
        }

        return {
            composer: $parent.children('.composer').val(),
            sizes: sizes.toString(),
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