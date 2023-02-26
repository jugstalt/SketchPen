// import * as monaco from 'monaco-editor';

function registerSketchPenLanguate() {
    console.log('register-sketchpen-language');
    monaco.languages.register({
        id: 'sketchpen',
    });

    let keywords = ['transform', 'path', 'line', 'circle', 'test1344444'];

    monaco.languages.setMonarchTokensProvider('sketchpen', {
        keywords: keywords,
        tokenizer: {
            root: [
                [/@?[a-zA-Z][\w$]*/, {
                    cases: {
                        '@keywords': 'keyword',
                        '@default': 'variable'
                    }
                }],
                [/".*?"/, 'string'],
                [/\/\//, 'comment'],
                [/[0-9]/, 'number']
            ]
        }
    });

    monaco.editor.defineTheme('sketchpen-theme', {
        base: 'vs',
        rules: [
            //{ token: 'keyword', foreground: '#ff6600', fontStyle: 'bold' },
            //{ token: 'comment', foreground: '#009900' },
            //{ token: 'string', foreground: '#ff9966' },
            //{ token: 'variable', foreground: '#006699' }
        ]
    });

    monaco.languages.registerCompletionItemProvider('sketchpen', {
        provideCompletionItems: (model, position) => {
            const suggestions = [
                {
                    label: "transform.reset()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "transform.reset();",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "transform.translate()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "transform.translate(${1:x}, ${2:y});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "transform.scale()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "transform.scale(${1:scale});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "transform.rotate()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "transform.rotate(${1:angle});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "line.draw()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "line.draw(${1:x1}, ${2:y1}, ${3:x2}, ${4:y2});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "line.draw()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "line.draw(${1:x1}, ${2:y1}, ${3:x2}, ${4:y2});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "path.begin()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "path.begin();",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "path.close()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "path.close();",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "path.addlines()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "path.addlines(${1:x1}, ${2:y1}, ${3:x2}, ${4:y2}, ...);",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "path.fill()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "path.fill(${1: optional color});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
                {
                    label: "path.draw()",
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: "path.draw(${1: optional color});",
                    insertTextRules:
                        monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                },
            ];
            return { suggestions: suggestions };
        }
    });
};
