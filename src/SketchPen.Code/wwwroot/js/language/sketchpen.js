// import * as monaco from 'monaco-editor';

function registerSketchPenLanguage(rules, globalVariables) {
    console.log('register-sketchpen-language', rules, globalVariables);

    monaco.languages.register({
        id: 'sketchpen',
    });

    let keywords = [];

    if (rules) {
        for (let keyword in rules) {
            keywords.push(keyword);
        }
    }

    monaco.languages.setMonarchTokensProvider('sketchpen', {
        keywords: keywords,
        tokenizer: {
            root: [
                [/@?[a-zA-Z][\w$]*/, {
                    cases: {
                        '@keywords': 'keyword',
                        //'@default': 'variable'
                    }
                }],
                [/".*?"/, 'string'],
                [/\/\//, 'comment'],
                [/[0-9]/, 'number'],
                [/^@@[a-zA-Z][a-zA-Z0-9]*$/, 'variable']
            ]
        }
    });

    monaco.editor.defineTheme('sketchpen-theme-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [
            { token: 'keyword', foreground: '#ff6600', fontStyle: 'bold' },
            { token: 'comment', foreground: '#009900' },
            { token: 'string', foreground: '#ff9966' },
            { token: 'variable', foreground: '#006699' }
        ]
    });

    monaco.languages.registerCompletionItemProvider('sketchpen', {
        triggerCharacters: ['.', '@'],
        provideCompletionItems: (model, position) => {

            if (!rules) return { suggestions: [] };

            //console.log('model', model);
            //console.log('position', position);

            let textUntilPosition = model.getValueInRange(
                {
                    startLineNumber: position.lineNumber,
                    startColumn: 1,
                    endLineNumber: position.lineNumber,
                    endColumn: position.column
                });
            const wordUntilPosition = model.getWordUntilPosition(position);

            console.log('textUntilPosition', textUntilPosition);
            console.log('wordUntilPosition', wordUntilPosition);

            let suggestions = [];

            if (textUntilPosition[textUntilPosition.length - 1] === '@') {
                for (var globalVariable of globalVariables) {
                    suggestions.push({
                        label: "@@"+globalVariable,
                        insertText: "@"+globalVariable,
                        kind: monaco.languages.CompletionItemKind.Varible,
                        insertTextRules:
                            monaco.languages.CompletionItemInsertTextRule.None
                    });
                }
            }
            else if (textUntilPosition.indexOf('.') > 0) {
                var keyword = textUntilPosition.trim().split('.')[0];

                if (rules[keyword]) {
                    for (var suggestion of rules[keyword]) {
                        suggestions.push({
                            label: suggestion.suggestion,
                            insertText: suggestion.snippet,
                            kind: monaco.languages.CompletionItemKind.Method,
                            insertTextRules:
                                monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                        });
                    }
                }
            } else {  // keyword
                for (var keyword of keywords) {
                    suggestions.push({
                        label: keyword,
                        insertText: keyword,
                        kind: monaco.languages.CompletionItemKind.Class,
                        insertTextRules:
                            monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
                    });
                }
            }

            //console.log(suggestions);

            return { suggestions: suggestions };
        }
    });
};

// https://ohdarling88.medium.com/4-steps-to-add-custom-language-support-to-monaco-editor-5075eafa156d
// https://codesandbox.io/s/monaco-demo-s0hei?file=/src/index.tsx
// https://stackoverflow.com/questions/51536492/monaco-editor-based-namespace-auto-complete