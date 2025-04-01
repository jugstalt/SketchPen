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
            $.error('Method ' + method + ' does not exist on jExt.sketchPenCode_tree');
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
        }
    };
    let initUI = function (parent, options) {
        let $parent = $(parent).addClass('sketchpen-code-tree-holder');

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

                    let $this=$(this), x = $(this).outerWidth() - e.originalEvent.layerX;
                    //console.log(x);
                    if (x < 8) {
                        $this.removeClass('has-value').val('');
                        setFilter($this.data('$tree'), '')
                    }
                })
                .on('keyup', function (e) {
                    let $this = $(this);
                    if ($this.val()) {
                        $this.addClass('has-value')
                    } else {
                        $this.removeClass('has-value');
                    }
                    setFilter($this.data('$tree'), $this.val())
                });
        }

        let $tree = createTreeNode("","<div>")
            .addClass('sketchpen-code-tree')
            .appendTo($parent);

        sketchPenCode.events.on('document-deleted', function (channel) {
            refresh($parent);
        });

        refresh($parent);
    };

    let setFilter = function ($parent, filter) {
        filter = filter.toLowerCase();

        if (!filter) {
            $parent.find('.tree-node').removeClass('hidden').removeClass('found').removeClass('collapsed');
            $parent.find('.tree-node').each(function (i, node) {
                let $node = $(node);
                if ($node.data('is_collapsed') === true) {
                    $node.addClass('collapsed');
                }
            });
            return;
        }

        $parent.find('.tree-node').each(function (i, node) {
            let $node = $(node);

            let searchText = $node.data('search-text');
            if (!searchText) {
                $node.addClass('hidden');
            } else if (searchText.indexOf(filter) < 0) {
                $node.addClass('hidden').removeClass('found');
            } else {
                $node.removeClass('hidden').addClass('found');

                //console.log(searchText, searchText.indexOf(filter));

                // Show all up nodes
                let $pNode = $node.parent().parent();
                while ($pNode.hasClass('tree-node')) {
                    $pNode.removeClass('hidden')
                          .addClass('collapsed');

                    $pNode = $pNode.parent().parent();
                }
            }
        });

        // Show all down nodes
        $parent.find('.tree-node.found').each(function (i, node) {
            let $node = $(node);

            $node
                .removeClass('collapsed')
                .find('.tree-node')
                .removeClass('hidden')
                .removeClass('collapsed');
        });
    }

    let refresh = function ($parent) {
        let $tree = $parent.children('.sketchpen-code-tree');

        let collapsedRoutes = [];
        $tree.find('.tree-node.collapsed').each(function (i, node) {
            collapsedRoutes.push($(node).data('data-route'));
        });

        $tree.empty();
        sketchPenCode.api.getFiles(function (filenames) {
            $.each(filenames, function (i, filename) {
                let routeParts = filename.split('/');

                let $parentNode = $tree;

                for (i = 0; i < routeParts.length - 1; i++) {
                    $parentNode = getOrCreateFolderNode($parentNode, routeParts[i], collapsedRoutes);
                }

                filename = routeParts[routeParts.length - 1];

                if (filename.indexOf(sketchPenCode.globalsFileExt()) > 0
                    && filename.indexOf(sketchPenCode.globalsFileExt()) === filename.length - sketchPenCode.globalsFileExt().length) {
                    console.log('globals-file:', filename);
                } else {
                    addFileNode($parentNode, filename, collapsedRoutes);
                }
            });
        });
    }

    let createTreeNode = function (label, element, asInput) {
        let $node = $(element || "<li>")
            .addClass("tree-node");

        if (label) {
            if (asInput == true) {
                $("<input type='text'/>")
                    .attr('placeholder', label)
                    .appendTo($node);
            } else {
                let $label = $("<div>")
                    .addClass('label')
                    .text(label)
                    .appendTo($node);

                $node
                    .addClass(sketchPenCode.fileClass(label))
                    .on('mousemove', function (e) {
                        $(this).closest('.sketchpen-code-tree-holder').find('.tree-node').removeClass('mouseover');
                        e.stopPropagation();
                        if (e.originalEvent.layerY >= 0 && e.originalEvent.layerY <= 32) {
                            $(this).addClass('mouseover');
                        } else {
                            $(this).removeClass('mouseover');
                        }
                    })
                    .on('mouseleave', function (e) {
                        $(this).removeClass('mouseover');
                    });

                let $removeButton = $("<div>")
                    .addClass('remove-button')
                    .appendTo($node)
                    .click(function (e) {
                        e.stopPropagation();

                        let route = $(this).closest('.tree-node').data('data-route');
                        var file = route.substr(sketchPenCode.id().length + 1);

                        sketchPenCode.events.fire('delete-document', { file: file });
                    });
            }
        }

        return $node;
    };

    let addToNodes = function ($node, $parent) {
        let $nodes = $parent.children('.tree-nodes');
        if ($nodes.length === 0) {
            $nodes = $("<ul>")
                .addClass('tree-nodes')
                .appendTo($parent);
        }
        $node.appendTo($nodes);
    }

    let getOrCreateFolderNode = function ($parent, folder, collapsedRoutes) {
        let $node = null;
        $parent.children('.tree-nodes').children('.folder').each(function (f, folderNode) {
            let $folderNode = $(folderNode);
            
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
                    let $this = $(this);
                    if (e.originalEvent.layerX < 30) {
                        $this.toggleClass('collapsed');
                        $this.data('is_collapsed', $this.hasClass('collapsed'));
                    } else {
                        //sketchPenCode.events.fire('open-endpoint', {
                        //    endpoint: $this.data('data-endpoint'),
                        //});
                    }
                }
            });
        }

        return $node;
    };

    let addFileNode = function ($parent, filename, collapsedRoutes) {
        let $node = createTreeNode(filename || 'New File...', null, filename === null)
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

                let $this = $(this);
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
                        let $this = $(this);

                        let id = $this.val();
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

})(jExt);
