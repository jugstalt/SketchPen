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
                .attr('placeholder', 'Find Endpoint, Query, View...')
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
        sketchPenCode.api.getEndPoints(function (endPoints) {
            if (sketchPenCode.privileges.createEndpoints()) {
                addEndPointNode($tree, null);
            }

            $.each(endPoints, function (i, endPoint) {
                addEndPointNode($tree, endPoint, collapsedRoutes);
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

    var addEndPointNode = function ($parent, endPoint, collapsedRoutes) {
        var $node = createTreeNode(endPoint || 'New endpoint...', null, endPoint === null)
            .addClass('endpoint')
            .data('data-endpoint', endPoint)
            .data('data-route', endPoint);

        if ($.inArray($node.data('data-route'), collapsedRoutes) >= 0) {
            $node.addClass('collapsed');
            $node.data('is_collapsed', true);
        }

        if (endPoint) {
            $node.data('search-text', endPoint.toLowerCase());
        }

        addToNodes($node, $parent);

        if (endPoint) {
            $node.addClass('loading-' + endPoint);
            sketchPenCode.api.getQueries($node.data('data-endpoint'), function (queries) {
                $node.removeClass('loading-' + endPoint);
                if (sketchPenCode.privileges.createQueries()) {
                    addQueryNode($node, $node.data('data-endpoint'), null);
                }

                $.each(queries, function (i, query) {
                    addQueryNode($node, $node.data('data-endpoint'), query, collapsedRoutes);
                });
            });
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
                        //console.log('create endpoint ' + id);
                        $this.val('');

                        sketchPenCode.api.createEndPoint(id, function (result) {
                            if (result.success == true) {
                                refresh($this.closest('.sketchpen-code-tree-holder'));
                            } else {
                                sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                            }
                        });
                    }
                });
        }
    };

    var addQueryNode = function ($parent, endPoint, query, collapsedRoutes) {
        var $node = createTreeNode(query || 'New query/data...', null, query === null)
            .addClass('query')
            .data('data-endpoint', endPoint)
            .data('data-query', query)
            .data('data-route', endPoint + '@' + query);

        if ($.inArray($node.data('data-route'), collapsedRoutes) >= 0) {
            $node.addClass('collapsed');
            $node.data('is_collapsed', true);
        }

        if (query) {
            $node.data('search-text', query.toLowerCase());
        }

        addToNodes($node, $parent);

        if (query) {
            var $endPointNode = $node.parent().parent();
            $endPointNode.addClass('loading-' + query).addClass('has-children');
            $node.addClass('loading-' + query);

            sketchPenCode.api.getViews($node.data('data-endpoint'), $node.data('data-query'), function (views) {
                $endPointNode.removeClass('loading-' + query);
                $node.removeClass('loading-' + query);

                if (views.length > 0) {
                    $node.addClass('has-children');
                }

                if (sketchPenCode.privileges.createViews()) {
                    addViewNode($node, $node.data('data-endpoint'), $node.data('data-query'), null);
                }

                $.each(views, function (i, view) {
                    addViewNode($node, $node.data('data-endpoint'), $node.data('data-query'), view);
                });
            });

            $node.click(function (e) {
                e.stopPropagation();
                if (e.originalEvent.layerY < 24) {
                    var $this = $(this);
                    if (e.originalEvent.layerX < 30) {
                        $this.toggleClass('collapsed');
                        $this.data('is_collapsed', $this.hasClass('collapsed'));
                    } else {
                        sketchPenCode.events.fire('open-query', {
                            endpoint: $this.data('data-endpoint'),
                            query: $this.data('data-query')
                        });
                    }
                }
            })
        } else {
            $node
                .addClass('add')
                .click(function (e) {
                    e.stopPropagation();
                })
                .find('input').on('keyup', function (e) {
                    if (e.which == 13) {
                        var $this = $(this), $node = $this.closest('.query');

                        var id = $this.val();
                        //console.log('create query ' + id);
                        $this.val('');

                        sketchPenCode.api.createQuery($node.data('data-endpoint'), id, function (result) {
                            if (result.success == true) {
                                refresh($this.closest('.sketchpen-code-tree-holder'));
                            } else {
                                sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                            }
                        });
                    }
                });
        }
    };

    var addViewNode = function ($parent, endPoint, query, view) {
        var $node = createTreeNode(view || 'New view...', null, view === null)
            .addClass('view')
            .data('data-endpoint', endPoint)
            .data('data-query', query)
            .data('data-view', view)
            .data('data-route', endPoint + '@' + query + '@' + view);

        if (view) {
            $node.data('search-text', view.toLowerCase());
        }

        addToNodes($node, $parent);

        if (view) {
            $node.click(function (e) {
                e.stopPropagation();

                var $this = $(this);
                sketchPenCode.events.fire('open-view', {
                    endpoint: $this.data('data-endpoint'),
                    query: $this.data('data-query'),
                    view: $this.data('data-view')
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
                        var $this = $(this), $node = $this.closest('.view');

                        var id = $this.val();
                        //console.log('create view ' + id);
                        $this.val('');

                        sketchPenCode.api.createView($node.data('data-endpoint'), $node.data('data-query'), id, function (result) {
                            if (result.success == true) {
                                refresh($this.closest('.sketchpen-code-tree-holder'));
                            } else {
                                sketchPenCode.ui.alert("Error", (result.error_message || 'Unknown error'));
                            }
                        });
                    }
                });
        }
    };
})(jQuery);
