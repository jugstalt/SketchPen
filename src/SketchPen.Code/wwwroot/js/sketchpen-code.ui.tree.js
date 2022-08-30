(function ($) {
    "use strict";
    $.fn.sketchPenCode_tree = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_tree');
        }
    };
    var defaults = {
        $toolbar: null,
    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        },
        refresh: function (options) {
            refresh($(this));
        }
    };
    var initUI = function (parent, options) {
        var $parent = $(parent).addClass('sketchpen-code-tree-holder');

        if (options.$toolbar) {
            $("<div>")
                .addClass('tree-tool expand-all')
                .data('$tree', $parent)
                .appendTo(options.$toolbar)
                .click(function (e) {
                    e.stopPropagation();
                    $(this).data('$tree')
                        .find('.tree-node.collapsed')
                        .each(function (i, node) {
                            $(node).removeClass('collapsed')
                                .data('is_collapsed', false);
                        });
                });

            $("<input>")
                .addClass('sketchpen-tree-search-input')
                .attr('placeholder', 'Find File...')
                .data('$tree', $parent)
                .appendTo(options.$toolbar)
                .click(function (e) {
                    e.stopPropagation();

                    var $this=$(this), x = $(this).outerWidth() - e.originalEvent.layerX;
                    //console.log(x);
                    if (x < 8) {
                        $this.removeClass('has-value').val('');
                        setFilter($this.data('$tree'), '')
                    }
                })
                .on('keyup', function (e) {
                    var $this = $(this);
                    if ($this.val()) {
                        $this.addClass('has-value')
                    } else {
                        $this.removeClass('has-value');
                    }
                    setFilter($this.data('$tree'), $this.val())
                });
        }

        var $tree = createTreeNode("","<div>")
            .addClass('sketchpen-code-tree')
            .appendTo($parent);

        sketchPenCode.events.on('document-deleted', function (channel) {
            refresh($parent);
        });

        refresh($parent);
    };

    var setFilter = function ($parent, filter) {
        filter = filter.toLowerCase();

        if (!filter) {
            $parent.find('.tree-node').removeClass('hidden').removeClass('found').removeClass('collapsed');
            $parent.find('.tree-node').each(function (i, node) {
                var $node = $(node);
                if ($node.data('is_collapsed') === true) {
                    $node.addClass('collapsed');
                }
            });
            return;
        }

        $parent.find('.tree-node').each(function (i, node) {
            var $node = $(node);

            var searchText = $node.data('search-text');
            if (!searchText) {
                $node.addClass('hidden');
            } else if (searchText.indexOf(filter) < 0) {
                $node.addClass('hidden').removeClass('found');
            } else {
                $node.removeClass('hidden').addClass('found');

                //console.log(searchText, searchText.indexOf(filter));

                // Show all up nodes
                var $pNode = $node.parent().parent();
                while ($pNode.hasClass('tree-node')) {
                    $pNode.removeClass('hidden')
                          .addClass('collapsed');

                    $pNode = $pNode.parent().parent();
                }
            }
        });

        // Show all down nodes
        $parent.find('.tree-node.found').each(function (i, node) {
            var $node = $(node);

            $node
                .removeClass('collapsed')
                .find('.tree-node')
                .removeClass('hidden')
                .removeClass('collapsed');
        });
    }

    var refresh = function ($parent) {
        var $tree = $parent.children('.sketchpen-code-tree');

        var collapsedRoutes = [];
        $tree.find('.tree-node.collapsed').each(function (i, node) {
            collapsedRoutes.push($(node).data('data-route'));
        });

        $tree.empty();
        sketchPenCode.api.getFiles(function (filenames) {
            $.each(filenames, function (i, filename) {
                var routeParts = filename.split('/');

                var $parentNode = $tree;
                for (i = 0; i < routeParts.length - 1; i++) {
                    $parentNode = getOrCreateFolderNode($parentNode, routeParts[i], collapsedRoutes);
                }

                var filename = routeParts[routeParts.length - 1];

                if (filename.indexOf('.globals') === filename.length - '.globals'.length) {

                } else {
                    addFileNode($parentNode, filename, collapsedRoutes);
                }
            });
        });
    }

    var createTreeNode = function (label, element, asInput) {
        var $node = $(element || "<li>")
            .addClass("tree-node");

        if (label) {
            $("<div>").addClass('icon').appendTo($node);
            if (asInput == true) {
                $("<input type='text'/>")
                    .attr('placeholder', label)
                    .appendTo($node);
            } else {
                var $label = $("<div>").addClass('label').text(label).appendTo($node);

                $node.on('mousemove', function (e) {
                    $(this).closest('.sketchpen-code-tree-holder').find('.tree-node').removeClass('mouseover');
                    e.stopPropagation();
                    if (e.originalEvent.layerY >= 0 && e.originalEvent.layerY <= 32) {
                        $(this).addClass('mouseover');
                    } else {
                        $(this).removeClass('mouseover');
                    }
                }).on('mouseleave', function (e) {
                    $(this).removeClass('mouseover');
                });

                var $copyButton = $("<div>")
                    .addClass('copy-button')
                    .appendTo($node)
                    .mouseout(function () {
                        $(this).find('.tooltiptext').removeClass('show');
                    })
                    .click(function (e) {
                        e.stopPropagation();

                        var route = $(this).closest('.tree-node').data('data-route');
                        navigator.clipboard.writeText(route);

                        if (route.length > 20)
                            route = route.substr(0, 20) + '...';

                        $(this)
                            .find('.tooltiptext')
                            .text("Copied route: " + route)
                            .addClass('show');
                    });

                $("<span>")
                    .addClass('tooltiptext')
                    .text('Copy placeholder')
                    .appendTo($copyButton);
            }
        }

        return $node;
    };

    var addToNodes = function ($node, $parent) {
        var $nodes = $parent.children('.tree-nodes');
        if ($nodes.length === 0) {
            $nodes = $("<ul>")
                .addClass('tree-nodes')
                .appendTo($parent);
        }
        $node.appendTo($nodes);
    }

    var getOrCreateFolderNode = function ($parent, folder, collapsedRoutes) {
        var $node = null;
        $parent.children('.tree-nodes').children('.folder').each(function (f, folderNode) {
            var $folderNode = $(folderNode);
            
            if ($folderNode.data('data-folder') === folder) {
                $node = $folderNode;
            }
        });

        if ($node == null) {
            $node = createTreeNode(folder)
                .addClass('folder')
                .data('data-folder', folder)
                .data('data-route', ($parent.data('data-route') || '') + folder + '/');

            if ($.inArray($node.data('data-route'), collapsedRoutes) >= 0) {
                $node.addClass('collapsed');
                $node.data('is_collapsed', true);
            }

            addToNodes($node, $parent);

            if (sketchPenCode.privileges.createFiles()) {
                addFileNode($node, null, collapsedRoutes);
            }

            $node.click(function (e) {
                e.stopPropagation();

                if (e.originalEvent.layerY < 24) {
                    var $this = $(this);
                    if (e.originalEvent.layerX < 30) {
                        $this.toggleClass('collapsed');
                        $this.data('is_collapsed', $this.hasClass('collapsed'));
                    } else {
                        sketchPenCode.events.fire('open-endpoint', {
                            endpoint: $this.data('data-endpoint'),
                        });
                    }
                }
            });
        }

        return $node;
    };

    var addFileNode = function ($parent, filename, collapsedRoutes) {
        var $node = createTreeNode(filename || 'New File...', null, filename === null)
            .addClass('file')
            .data('data-filename', filename)
            .data('data-route', ($parent.data('data-route')  || '') + (filename || ''));

        if ($.inArray($node.data('data-route'), collapsedRoutes) >= 0) {
            $node.addClass('collapsed');
            $node.data('is_collapsed', true);
        }

        if (filename) {
            $node.data('search-text', filename.toLowerCase());
        }

        addToNodes($node, $parent);

        if (filename) {
            $node.click(function (e) {
                e.stopPropagation();

                var $this = $(this);
                sketchPenCode.events.fire('open-file', {
                    route: $this.data('data-route'),
                });
            });
        } else {
            $node
                .addClass('add')
                .click(function (e) {
                    e.stopPropagation();
                })
                .find('input').on('keyup', function (e) {
                    if (e.which == 13) {
                        var $this = $(this);

                        var id = $this.val();
                        //console.log('create file ' + id);
                        $this.val('');

                        sketchPenCode.api.createFile(id, function (result) {
                            if (result.success == true) {
                                refresh($this.closest('.sketchpen-code-tree-holder'));
                                if (result.route) {
                                    sketchPenCode.events.fire('open-file', {
                                        route: result.route
                                    });
                                }
                            } else {
                                sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                            }
                        });
                    }
                });
        }
    };

})(jQuery);
