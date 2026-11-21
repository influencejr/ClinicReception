#!/usr/bin/env bash
# Правила стандарту, яких не перевіряють аналізатори .NET:
# A-05 - придушення директивою #pragma заборонено (лише .editorconfig або SuppressMessage),
# C-04 - TODO тільки з ідентифікатором боргу (TODO TD-09: ...),
# S-06 - goto заборонено.
# Запуск: bash scripts/check-source.sh (те саме робить крок конвеєра CI).

status=0

report() {
    if [ -n "$2" ]; then
        echo "Порушено $1:"
        echo "$2"
        status=1
    fi
}

report "A-05 (#pragma warning disable)" "$(git grep -n '#pragma warning disable' -- '*.cs')"
report "C-04 (TODO без TD-)" "$(git grep -n 'TODO' -- '*.cs' | grep -v 'TODO TD-[0-9]')"
report "S-06 (goto)" "$(git grep -nw 'goto' -- '*.cs')"

if [ "$status" -eq 0 ]; then
    echo "Заборонених конструкцій не знайдено"
fi

exit "$status"
