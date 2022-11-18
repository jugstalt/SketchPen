(function ($) {
    "use strict";
    $.fn.sketchPenCode_editor = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_editor');
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
        },
        currentDoc: function (options) {
            let $tabs = $(this).children('.sketchpen-code-tabs')

            return $tabs.children(".sketchpen-code-tab.selected").attr('data-route');
        },
        dirtyDocs: function (options) {
            let ids = [];
            $(this).children('.sketchpen-code-tabs').children(".sketchpen-code-tab.dirty").each(function (i, e) {
                ids.push($(e).attr('data-route'));
            });
            return ids;
        },
        addTab: function (options) {
            showOrAddTab($(this).children('.sketchpen-code-tabs'), options.route, options.className, options.hideCloseButton)
        },
        isOpen: function (options) {
            return $(this).children('.sketchpen-code-tabs').children(".sketchpen-code-tab[data-route='" + options.route + "']").length > 0;
        }
    };
    let initUI = function (parent, options) {
        let $parent = $(parent);

        let $tabs = $("<div>")
            .addClass('sketchpen-code-tabs')
            .appendTo($parent);

        $("<div>")
            .addClass('sketchpen-code-tab-selector')
            .appendTo($tabs)
            .click(function () {
                $('body').sketchPen_code_modal({
                    title: 'Open tabs...',
                    onload: function ($content) {
                        renderOpenTabs($tabs, $content);
                    }
                });
            });

        let $editor = $("<div>")
            .addClass('sketchpen-code-editor')
            .appendTo($parent);

        sketchPenCode.events.on('open-file', function (channel, args) {
            let $tab = showOrAddTab($tabs, args.route, 'file');
        });

        sketchPenCode.events.on('tab-selected', function (channel, args) {
            showOrAddEditorFrame($editor, args.route);

            checkSize($tabs);
            sketchPenCode.events.fire('refresh-ui');
        });
        sketchPenCode.events.on('tab-removed', function (channel, args) {
            sketchPenCode.events.fire('destroy-editor', { id: args.route });
            $(".sketchpen-code-editor-frame[data-route='" + args.route + "']").remove();

            if (args.selected) {
                $tabs.children('.sketchpen-code-tab').last().trigger('click');
            }

            checkSize($tabs);
            sketchPenCode.events.fire('refresh-ui');
        });

        sketchPenCode.events.on('document-changed', function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-route='" + args.route + "']")
                 .addClass('dirty');

            sketchPenCode.events.fire('refresh-ui');
        });

        sketchPenCode.events.on(['save-document','verify-document'], function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-route='" + args.route + "']")
                .addClass('loading');
        });
        sketchPenCode.events.on(['document-saved', 'document-verified'], function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-route='" + args.route + "']")
                .removeClass('loading')
                .removeClass('errors');

            if (channel.channel === 'document-saved') {
                $tabs.children(".sketchpen-code-tab[data-route='" + args.route + "']")
                    .removeClass('dirty');
            }

            sketchPenCode.events.fire('refresh-ui');
        });
        sketchPenCode.events.on('document-errors', function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-route='" + args.route + "']").removeClass('loading').addClass('errors');
        });

        sketchPenCode.events.on('ide-resize', function (channel, args) {
            checkSize($tabs);
        });

        sketchPenCode.events.on('document-deleted', function (channel, args) {
            $parent.children('.sketchpen-code-tabs').children('.sketchpen-code-tab').each(function (i, tab) {
                let $tab = $(tab);
                let id = $tab.attr('data-route');
                if (id === args.route || id.indexOf(args.route + '@') === 0) {
                    let selected = $tab.hasClass('selected');
                    $tab.remove();
                    sketchPenCode.events.fire('tab-removed', { id: id, selected: selected });
                };
            });
            $(".sketchpen-code-editor-frame[data-route='" + args.route + "']").remove();

            sketchPenCode.events.fire('refresh-ui');
        });
    };

    let showOrAddTab = function ($tabs, route, cls, hideCloseButton) {
        let $tab = $tabs.children(".sketchpen-code-tab[data-route='" + route + "']");
        if ($tab.length === 0) {

            let routeParts = route.split('/');

            $tab = $("<div>")
                .addClass('sketchpen-code-tab')
                .attr('data-route', route)
                .text(routeParts[routeParts.length - 1])
                .appendTo($tabs);

            if (cls) {
                $tab.addClass(cls);
            }

            if (!hideCloseButton) {
                $("<div>")
                    .addClass('close-button')
                    .appendTo($tab)
                    .click(function (e) {
                        e.stopPropagation();

                        let $tab = $(this).parent();
                        let id = $tab.attr('data-route');

                        sketchPenCode.ui.confirmIf(
                            $tab.hasClass('dirty'),
                            route,
                            "Close tab without saving? You will loose all changes!",
                            function () {
                                let selected = $tab.hasClass('selected');
                                $tab.remove();

                                sketchPenCode.events.fire('tab-removed', { id: id, selected: selected });
                            }
                        )
                    });
            }
        }

        $tab.click(function (e) {
            e.stopPropagation();

            $tabs.children('.selected').removeClass('selected');
            $(this).addClass('selected');

            sketchPenCode.events.fire('tab-selected', { route: $(this).attr('data-route') });
        });

        $tab.trigger('click');

        checkSize($tabs);

        return $tab;
    };

    let renderOpenTabs = function ($tabs, $parent) {
        let $ul = $("<ul>")
            .addClass('sketchpen-code-open-tabs')
            .appendTo($parent);

        let menuRowAdded = false;

        $tabs.children('.sketchpen-code-tab').each(function (i, tab) {
            let $tab = $(tab);

            let id = $tab.attr('data-route');

            if (menuRowAdded == false && id.indexOf('_') != 0) {
                menuRowAdded = true;

                let $menu = $("<li>")
                    .addClass('sketchpen-code-tab')
                    .css('text-align', 'right')
                    .appendTo($ul);

                $("<button>")
                    .addClass('sketchpen-code-button cancel')
                    .text('Close selected')
                    .appendTo($menu)
                    .click(function () {
                        $(this).closest('.sketchpen-code-open-tabs').children('.sketchpen-code-tab').each(function (i, li) {
                            let $li = $(li), $tab = $li.data("$tab");

                            if (!$tab || $tab.attr('data-route').indexOf('_') == 0) {
                                return;
                            }

                            let $checkbox = $li.children('.checkbox');

                            if ($checkbox.hasClass('checked') === true) {
                                $tab.children('.close-button').trigger('click');
                            }
                        });

                        $(null).sketchPen_code_modal('close');
                    });

                $("<div>")
                    .addClass('checkbox')
                    .appendTo($menu)
                    .click(function (e) {
                        e.stopPropagation();

                        let $this = $(this);
                        $this.toggleClass('checked');
                        let checked = $this.hasClass('checked');

                        $this.closest('.sketchpen-code-open-tabs').children('.sketchpen-code-tab').each(function (i, li) {
                            let $li = $(li), $tab = $li.data("$tab");

                            if (!$tab || $tab.attr('data-route').indexOf('_') == 0) {
                                return;
                            }

                            let $checkbox = $li.children('.checkbox');

                            if (($checkbox.hasClass('checked') === true && checked === false) ||
                                ($checkbox.hasClass('checked') === false && checked === true)) {
                                $checkbox.trigger('click');
                            }
                        });
                    });
            }

            let $li = $("<li>")
                .data("$tab", $tab)
                .attr('class', $tab.attr('class'))
                .click(function (e) {
                    e.stopPropagation();
                    $(this).data("$tab").trigger('click');
                    $(null).sketchPen_code_modal('close');
                })
                .appendTo($ul);

            $("<div>")
                .addClass('text')
                .text($tab.text())
                .appendTo($li);

            if (id.indexOf('_') != 0) {
                $("<div>")
                    .addClass('subtext')
                    .text(id)
                    .appendTo($li);

                $("<div>")
                    .addClass('checkbox')
                    .appendTo($li)
                    .click(function (e) {
                        e.stopPropagation();

                        let $this = $(this);
                        if ($this.parent().hasClass('dirty') || $this.parent().hasClass('errors')) {
                            return;
                        }

                        $this.toggleClass('checked');
                    });
            }
        });
    };

    let checkSize = function ($tabs) {
        function check(skip) {
            let pos = 0, tabsWidth = $tabs.width();

            $tabs.children('.sketchpen-code-tab').each(function (i, tab) {
                let $tab = $(tab).css('display', ''), tabWidth = $tab.outerWidth();

                if (i < skip) {
                    $tab.css('display', 'none');
                } else {
                    if (pos + tabWidth >= tabsWidth - 30) {
                        $tab.css('display', 'none');
                    } else {
                        pos += tabWidth;
                    }
                }
            });
        };

        let numTabs = $tabs.children('.sketchpen-code-tab').length, skip = 0;
        let $selectedTab = $tabs.children('.sketchpen-code-tab.selected');

        while (skip < numTabs) {
            check(skip);

            if ($selectedTab.length === 0 || $selectedTab.css('display') !== 'none') {
                break;
            }

            skip++;
        }
    };

    let showOrAddEditorFrame = function ($editor, route) {
        let $frame = $editor.children(".sketchpen-code-editor-frame[data-route='" + route + "']");

        if ($frame.length === 0) {
            let src = '';

            if (route === '_start') {
                src = sketchPenCode.targetUrl() + '/Start';
            } else {
                src = sketchPenCode.targetUrl() + '/EditFile?route=' + route;
            }

            $frame = $("<iframe>")
                .addClass('sketchpen-code-editor-frame')
                .attr('data-route', route)
                .attr('src', src)
                .appendTo($editor);
        }

        $editor.children('.selected').removeClass('selected');
        $frame.addClass('selected');
    };
})(jQuery);
