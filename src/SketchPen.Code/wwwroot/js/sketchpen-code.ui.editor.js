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
    var defaults = {
        
    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        },
        currentDoc: function (options) {
            var $tabs = $(this).children('.sketchpen-code-tabs')

            return $tabs.children(".sketchpen-code-tab.selected").attr('data-id');
        },
        dirtyDocs: function (options) {
            var ids = [];
            $(this).children('.sketchpen-code-tabs').children(".sketchpen-code-tab.dirty").each(function (i, e) {
                ids.push($(e).attr('data-id'));
            });
            return ids;
        },
        addTab: function (options) {
            showOrAddTab($(this).children('.sketchpen-code-tabs'), options.title, options.id, options.className, options.hideCloseButton)
        },
        isOpen: function (options) {
            return $(this).children('.sketchpen-code-tabs').children(".sketchpen-code-tab[data-id='" + options.id + "']").length > 0;
        }
    };
    var initUI = function (parent, options) {
        var $parent = $(parent);

        var $tabs = $("<div>")
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

        var $editor = $("<div>")
            .addClass('sketchpen-code-editor')
            .appendTo($parent);

        sketchPenCode.events.on('open-endpoint', function (channel, args) {
            var $tab = showOrAddTab($tabs, args.endpoint, args.endpoint, 'endpoint');
        });
        sketchPenCode.events.on('open-query', function (channel, args) {
            var $tab = showOrAddTab($tabs, args.query, args.endpoint + '@' + args.query, 'query');
        });
        sketchPenCode.events.on('open-view', function (channel, args) {
            var $tab = showOrAddTab($tabs, args.view, args.endpoint + '@' + args.query + '@' + args.view, 'view');
        });
        sketchPenCode.events.on('open-endpoint-css', function (channel, args) {
            var $tab = showOrAddTab($tabs, 'CSS: ' + args.id, args.id + '@_css', 'css');
        });

        sketchPenCode.events.on('tab-selected', function (channel, args) {
            showOrAddEditorFrame($editor, args.id);

            checkSize($tabs);
            sketchPenCode.events.fire('refresh-ui');
        });
        sketchPenCode.events.on('tab-removed', function (channel, args) {
            sketchPenCode.events.fire('destroy-editor', { id: args.id });
            $(".sketchpen-code-editor-frame[data-id='" + args.id + "']").remove();

            if (args.selected) {
                $tabs.children('.sketchpen-code-tab').last().trigger('click');
            }

            checkSize($tabs);
            sketchPenCode.events.fire('refresh-ui');
        });

        sketchPenCode.events.on('document-changed', function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-id='" + args.id + "']")
                .addClass('dirty');

            sketchPenCode.events.fire('refresh-ui');
        });

        sketchPenCode.events.on(['save-document','verify-document'], function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-id='" + args.id + "']")
                .addClass('loading');
        });
        sketchPenCode.events.on(['document-saved', 'document-verified'], function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-id='" + args.id + "']")
                .removeClass('loading')
                .removeClass('errors');

            if (channel.channel === 'document-saved') {
                $tabs.children(".sketchpen-code-tab[data-id='" + args.id + "']")
                    .removeClass('dirty');
            }

            sketchPenCode.events.fire('refresh-ui');
        });
        sketchPenCode.events.on('document-errors', function (channel, args) {
            $tabs.children(".sketchpen-code-tab[data-id='" + args.id + "']").removeClass('loading').addClass('errors');
        });

        sketchPenCode.events.on('ide-resize', function (channel, args) {
            checkSize($tabs);
        });

        sketchPenCode.events.on('document-deleted', function (channel, args) {
            $parent.children('.sketchpen-code-tabs').children('.sketchpen-code-tab').each(function (i, tab) {
                var $tab = $(tab);
                var id = $tab.attr('data-id');
                if (id === args.id || id.indexOf(args.id + '@') === 0) {
                    var selected = $tab.hasClass('selected');
                    $tab.remove();
                    sketchPenCode.events.fire('tab-removed', { id: id, selected: selected });
                };
            });
            $(".sketchpen-code-editor-frame[data-id='" + args.id + "']").remove();

            sketchPenCode.events.fire('refresh-ui');
        });
    };

    var showOrAddTab = function ($tabs, title, id, cls, hideCloseButton) {
        var $tab = $tabs.children(".sketchpen-code-tab[data-id='" + id + "']");
        if ($tab.length === 0) {
            $tab = $("<div>")
                .addClass('sketchpen-code-tab')
                .attr('data-id', id)
                .text(title)
                .appendTo($tabs)

            if (cls) {
                $tab.addClass(cls);
            }

            if (!hideCloseButton) {
                $("<div>")
                    .addClass('close-button')
                    .appendTo($tab)
                    .click(function (e) {
                        e.stopPropagation();

                        var $tab = $(this).parent();
                        var id = $tab.attr('data-id');

                        sketchPenCode.ui.confirmIf(
                            $tab.hasClass('dirty'),
                            id,
                            "Close tab without saving? You will loose all changes!",
                            function () {
                                var selected = $tab.hasClass('selected');
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

            sketchPenCode.events.fire('tab-selected', { id: $(this).attr('data-id') });
        });

        $tab.trigger('click');

        checkSize($tabs);

        return $tab;
    };

    var renderOpenTabs = function ($tabs, $parent) {
        var $ul = $("<ul>")
            .addClass('sketchpen-code-open-tabs')
            .appendTo($parent);

        var menuRowAdded = false;

        $tabs.children('.sketchpen-code-tab').each(function (i, tab) {
            var $tab = $(tab);

            var id = $tab.attr('data-id');

            if (menuRowAdded == false && id.indexOf('_') != 0) {
                menuRowAdded = true;

                var $menu = $("<li>")
                    .addClass('sketchpen-code-tab')
                    .css('text-align', 'right')
                    .appendTo($ul);

                $("<button>")
                    .addClass('sketchpen-code-button cancel')
                    .text('Close selected')
                    .appendTo($menu)
                    .click(function () {
                        $(this).closest('.sketchpen-code-open-tabs').children('.sketchpen-code-tab').each(function (i, li) {
                            var $li = $(li), $tab = $li.data("$tab");

                            if (!$tab || $tab.attr('data-id').indexOf('_') == 0) {
                                return;
                            }

                            var $checkbox = $li.children('.checkbox');

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

                        var $this = $(this);
                        $this.toggleClass('checked');
                        var checked = $this.hasClass('checked');

                        $this.closest('.sketchpen-code-open-tabs').children('.sketchpen-code-tab').each(function (i, li) {
                            var $li = $(li), $tab = $li.data("$tab");

                            if (!$tab || $tab.attr('data-id').indexOf('_') == 0) {
                                return;
                            }

                            var $checkbox = $li.children('.checkbox');

                            if (($checkbox.hasClass('checked') === true && checked === false) ||
                                ($checkbox.hasClass('checked') === false && checked === true)) {
                                $checkbox.trigger('click');
                            }
                        });
                    });
            }

            var $li = $("<li>")
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

                        var $this = $(this);
                        if ($this.parent().hasClass('dirty') || $this.parent().hasClass('errors')) {
                            return;
                        }

                        $this.toggleClass('checked');
                    });
            }
        });
    };

    var checkSize = function ($tabs) {
        function check(skip) {
            var pos = 0, tabsWidth = $tabs.width();

            $tabs.children('.sketchpen-code-tab').each(function (i, tab) {
                var $tab = $(tab).css('display', ''), tabWidth = $tab.outerWidth();

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

        var numTabs = $tabs.children('.sketchpen-code-tab').length, skip = 0;
        var $selectedTab = $tabs.children('.sketchpen-code-tab.selected');

        while (skip < numTabs) {
            check(skip);

            if ($selectedTab.length === 0 || $selectedTab.css('display') !== 'none') {
                break;
            }

            skip++;
        }
    };

    var showOrAddEditorFrame = function ($editor, id) {
        var $frame = $editor.children(".sketchpen-code-editor-frame[data-id='" + id + "']");
        if ($frame.length === 0) {
            if (id === '_start') {
                src = sketchPenCode.targetUrl() + '/Start';
            } else {
                var ids = id.split('@'), src = '';
                if (ids.length === 1) {
                    src = sketchPenCode.targetUrl() + '/EditEndPoint?endpoint=' + ids[0];
                } else if (ids.length === 2) {
                    if (ids[1] == '_css') {
                        src = sketchPenCode.targetUrl() + '/EditEndPointCss?endpoint=' + ids[0];
                    } else {
                        src = sketchPenCode.targetUrl() + '/EditEndPointQuery?endpoint=' + ids[0] + '&query=' + ids[1];
                    }
                } else if (ids.length === 3) {
                    src = sketchPenCode.targetUrl() + '/EditEndPointQueryView?endpoint=' + ids[0] + '&query=' + ids[1] + '&view=' + ids[2];
                } else {
                    sketchPenCode.ui.alert('Error', 'unknown sketchpen route/id: ' + id);
                }
            }

            $frame = $("<iframe>")
                .addClass('sketchpen-code-editor-frame')
                .attr('data-id', id)
                .attr('src', src)
                .appendTo($editor);
        }

        $editor.children('.selected').removeClass('selected');
        $frame.addClass('selected');
    };
})(jQuery);
