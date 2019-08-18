namespace CMDASTView.DomainTypes

//
// D O M A I N   T Y P E S
// =======================
//
// This module manages each of the domain types used when translating
// CMD AST tokens in to an output visualisation.
//
//
type Operator =
    | Or
    | Success
    | Pipe
    | Always

type IfComparison   = IfComparison of string * string * string
type IfStatement    = IfStatement
type ForLoop        = ForLoop of string
type IfElse         = IfElse

type SpecialToken =
    | For of ForLoop
    | If  of IfStatement
    | IfTest of IfComparison
    | Else of IfElse
    | Binop of Operator

type Cmd = {
    Program: string
    Args: string option
}

type Token =
    | Special of SpecialToken
    | Command of Cmd


type ASTNode =
    | LeafNode           of Cmd
    | IfNode             of IfComparison * ASTNode * ASTNode option
    | ElseNode           of ASTNode
    | ForLoopNode        of ForLoop * ASTNode
    | BinaryOperatorNode of Operator * ASTNode * ASTNode
